using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// 미니언 피해 측정 (MinionTestWindow에서 실행, InGame Play 중, 에디터 전용)
// 미니언 1마리를 칸 (2,2)(왼쪽 위 TileGroup의 오른쪽 아래)에 두고 지정 wave를 처음부터 끝까지(적 전부 처리 또는 wave 시간 초과) 돌려
// 그 미니언이 준 피해(DamageMeterSystem)를 잰다. 머지 레벨 0~4를 차례로, 종 하나 또는 전체.
// 결과는 Library/MinionDamageTest.json에 쌓이고(같은 난이도·wave·key·레벨은 덮어씀) 측정마다
// Assets/MinionDamageTest/MinionDamage_{난이도}_Wave{N}.xlsx 를 다시 만든다 (요약 + 상세 시트)
// 측정 중: 킹 무적·킹 공격 끔·권능 끔(측정 시작 시 정리, 다시 켜지 않음 → 측정 후엔 Play를 다시 시작 권장)
// (2026-11-22) 킹 성장 능력(종족·직업·패시브 등)도 측정 시작 시 전부 제거(다시 안 생김) → 킹 패시브 공격·버프 칸 없이 미니언만
//   속도 = 항상 최대(TIME_SCALE), 엘리트·보스 없이 옵션(noEliteBoss = 엘리트·보스 wave도 일반 전투로, 파일 이름 _NoEliteBoss)
//   상성 무시 옵션(noDefence = 방어 타입 배율 전부 1, DefenceInfo.ignoreForTest, 파일 이름 _NoDefence)
public static class MinionDamageBenchmark
{
    public static readonly Vector2Int TILE_INDEX = new Vector2Int(2, 2);
    public const string OUTPUT_FOLDER = "Assets/MinionDamageTest";
    const string STORE_PATH = "Library/MinionDamageTest.json";
    const int RANDOM_SEED = 1234;
    const float EXTRA_TIMEOUT = 30f; // wave 시간 + 이만큼 지나도 안 끝나면 강제 종료 (게임 시간)
    public const float TIME_SCALE = 10f; // 측정 속도 (항상 최대. 너무 올리면 프레임당 이동이 커져 판정이 부정확해짐)

    [Serializable]
    public class Result
    {
        public string difficulty;
        public int wave; // 표시 번호
        public string key;
        public int mergeLevel;
        public float attackPower;
        public float attackSpeed;
        public double damage;      // 이 미니언(key)이 준 피해
        public double totalDamage; // 모든 미니언 피해 (성장 등으로 종이 바뀌면 이쪽이 실제 값)
        public float duration;     // wave 시작 ~ 끝 (게임 시간 초)
        public int kills;
        public int leaked;         // 경로 끝 도착
        public bool timeout;
        public string measuredAt;
        public bool noEliteBoss;   // 엘리트·보스 없이 측정
        public bool noDefence;     // 상성(방어 타입 배율) 무시
    }

    [Serializable]
    class Store { public List<Result> results = new(); }

    enum State { Idle, Prepare, Running }

    static State state = State.Idle;
    static readonly Queue<(string key, int lv)> queue = new();
    static (string key, int lv) current;
    static int waveNum;
    static float speed;
    static float prevTimeScale;
    static float startGameTime;
    static int kills, leaked;
    static float attackPower, attackSpeed;
    static int totalCount;
    static bool subscribed;
    static bool warnedFileLocked;
    static bool noEliteBoss;
    static bool noDefence;

    public static bool IsRunning => state != State.Idle;
    public static string Progress => IsRunning
        ? $"{totalCount - queue.Count}/{totalCount}  {current.key} 머지 {current.lv}  ({(state == State.Running ? $"{GameManager.Instance.gameTimer - startGameTime:0}초" : "준비")})"
        : "";

    public static string GetOutputPath(int wave, bool noEliteBoss, bool noDefence)
    {
        string diff = GameManager.Instance != null && GameManager.Instance.stageData != null ? GameManager.Instance.stageData.key : UserDataManager.difficultyType.ToString();
        return MakePath(diff, wave, noEliteBoss, noDefence);
    }

    static string MakePath(string difficulty, int wave, bool noEliteBoss, bool noDefence)
        => $"{OUTPUT_FOLDER}/MinionDamage_{difficulty}_Wave{wave}{(noEliteBoss ? "_NoEliteBoss" : "")}{(noDefence ? "_NoDefence" : "")}.xlsx";

    public static void Run(IEnumerable<string> keys, int wave, bool withoutEliteBoss, bool ignoreDefence = false)
    {
        if (IsRunning)
            return;
        GameManager gm = GameManager.Instance;
        if (gm == null || !gm.IsGameStarted || !gm.isPlaying)
        {
            Debug.LogWarning("[MinionDamage] 게임이 시작된 뒤에 실행하세요");
            return;
        }
        queue.Clear();
        foreach (string key in keys)
            for (int lv = 0; lv <= MinionData.MAX_MERGE_LEVEL; lv++)
                queue.Enqueue((key, lv));
        totalCount = queue.Count;
        if (totalCount == 0)
            return;

        waveNum = Mathf.Clamp(wave, 1, GameSetting.LAST_WAVE);
        speed = TIME_SCALE;
        noEliteBoss = withoutEliteBoss;
        GameManager.forceBattleWaveForTest = noEliteBoss;
        noDefence = ignoreDefence;
        DefenceInfo.ignoreForTest = noDefence;
        prevTimeScale = Time.timeScale;
        warnedFileLocked = false;

        AuthoritySystem.End(); // 권능 효과 없이 미니언만 측정
        KingSlime.Instance.enabled = false; // 킹 공격(Weapon) 끔
        // 킹 성장 능력 전부 제거 (패시브 공격·버프 칸 없이 미니언만 측정, 다시 안 생김)
        foreach (KingAbility a in KingSlime.Instance.abilityInventory.Abilities.ToList())
            KingSlime.Instance.abilityInventory.Remove(a.data);
        if (!subscribed)
        {
            GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
            subscribed = true;
        }
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        NextRun();
        Debug.Log($"[MinionDamage] 측정 시작: {totalCount}회 (wave {waveNum}, x{speed})");
    }

    public static void Stop()
    {
        if (!IsRunning)
            return;
        Finish("중지");
    }

    static void OnPlayModeChanged(PlayModeStateChange s)
    {
        if (s == PlayModeStateChange.ExitingPlayMode && IsRunning)
            Finish("Play 종료로 중지");
    }

    static void Finish(string reason)
    {
        state = State.Idle;
        queue.Clear();
        EditorApplication.update -= Update;
        if (subscribed)
        {
            GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
            subscribed = false;
        }
        GameManager.forceBattleWaveForTest = false;
        DefenceInfo.ignoreForTest = false;
        if (Application.isPlaying)
        {
            Time.timeScale = prevTimeScale;
            if (KingSlime.Instance != null)
                KingSlime.Instance.enabled = true;
        }
        AssetDatabase.Refresh();
        Debug.Log($"[MinionDamage] {reason} → {GetOutputPathSafe()}");
    }

    static string GetOutputPathSafe() => GameManager.Instance != null ? GetOutputPath(waveNum, noEliteBoss, noDefence) : OUTPUT_FOLDER;

    static void NextRun()
    {
        if (queue.Count == 0)
        {
            Finish("측정 완료");
            return;
        }
        current = queue.Dequeue();
        state = State.Prepare;
    }

    static void Update()
    {
        if (!Application.isPlaying || GameManager.Instance == null || KingSlime.Instance == null)
            return;
        GameManager gm = GameManager.Instance;
        KingSlime king = KingSlime.Instance;
        king.enabled = false; // 킹 공격 끔 (다른 곳에서 켜도 측정 중엔 계속 끔)
        if (king.health != null && king.health.curHp < king.MaxHp)
            king.health.curHp = king.MaxHp; // 무적 (시간 초과 피해로 죽지 않게)
        Time.timeScale = speed;

        switch (state)
        {
            case State.Prepare:
                if (!gm.isPlaying)
                {
                    Finish("게임이 진행 중이 아니라 중지");
                    return;
                }
                StartRun(gm);
                break;
            case State.Running:
                float elapsed = gm.gameTimer - startGameTime;
                if (gm.isBreakTime || elapsed > GameSetting.WAVE_TIME + EXTRA_TIMEOUT)
                {
                    EndRun(gm, timeout: !gm.isBreakTime);
                    NextRun();
                }
                break;
        }
    }

    static void StartRun(GameManager gm)
    {
        CleanField(gm);

        // 킹이 측정 칸에 있으면 비켜 둠
        Tile tile = TileManager.Instance.GetTile(TILE_INDEX);
        if (tile == null)
        {
            Finish($"칸 {TILE_INDEX} 없음");
            return;
        }
        if (tile.isBlocked)
            tile.Unblock();
        if (ReferenceEquals(tile.tileObject, KingSlime.Instance))
        {
            Tile other = TileManager.Instance.tiles.LastOrDefault(t => t.IsEmpty && t != tile);
            TileManager.Instance.RemoveTileObject(KingSlime.Instance);
            other?.SetTileObject(KingSlime.Instance);
        }
        if (tile.tileObject != null)
        {
            Finish($"칸 {TILE_INDEX}이 비어 있지 않음");
            return;
        }

        UnityEngine.Random.InitState(RANDOM_SEED);
        Minion minion = KingSlime.Instance.AddMinion(current.key, current.lv);
        tile.SetTileObject(minion);
        attackPower = minion.statDic.TryGetValue(StatType.AttackPower, out float ap) ? ap : 0;
        attackSpeed = minion.AttackSpeed;

        DamageMeterSystem.Clear();
        kills = leaked = 0;
        startGameTime = gm.gameTimer;
        // 브레이크타임 → Next와 같은 경로로 wave 시작
        gm.isBreakTime = true;
        gm.breakTimeNextWaveIdx = waveNum - 1;
        gm.EndBreakTime();
        state = State.Running;
    }

    // 이전 측정의 wave·적·미니언·지뢰 정리
    static void CleanField(GameManager gm)
    {
        Wave wave = gm.CurrentWave;
        if (wave != null && !wave.isEnd)
        {
            wave.isSpawnEnd = true;
            wave.EndWave();
        }
        EnemyFieldController.Instance.ClearAllEnemies();
        foreach (Minion m in MinionFieldController.Instance.activeminions.ToList())
            if (m != null && TileManager.Instance.GetTile(m) != null)
                MinionFieldController.Instance.RemoveFieldMinion(m);
        // 지뢰는 미니언이 사라져도 남으므로 회수
        foreach (Mine mine in UnityEngine.Object.FindObjectsByType<Mine>(FindObjectsSortMode.None))
            mine.SendMessage("Release", SendMessageOptions.DontRequireReceiver);
    }

    static void OnEnemyDead(EnemyDeadEvent e)
    {
        if (state != State.Running || e.enemy == null)
            return;
        if (e.enemy.CurHp <= 0)
            kills++;
        else
            leaked++;
    }

    static void EndRun(GameManager gm, bool timeout)
    {
        MinionData data = MinionManager.Instance.minionDataDic.TryGetValue(current.key, out MinionData d) ? d : null;
        var result = new Result
        {
            difficulty = gm.stageData != null ? gm.stageData.key : "",
            wave = waveNum,
            key = current.key,
            mergeLevel = current.lv,
            attackPower = attackPower,
            attackSpeed = attackSpeed,
            damage = DamageMeterSystem.GetDamage(current.key),
            totalDamage = DamageMeterSystem.TotalDamage,
            duration = gm.gameTimer - startGameTime,
            kills = kills,
            leaked = leaked,
            timeout = timeout,
            measuredAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            noEliteBoss = noEliteBoss,
            noDefence = noDefence,
        };
        if (timeout)
        {
            // wave가 안 끝났으면 정리해 두고 다음 측정에서 새로 시작
            Wave wave = gm.CurrentWave;
            if (wave != null && !wave.isEnd)
                wave.EndWave();
        }
        Debug.Log($"[MinionDamage] {result.key} 머지 {result.mergeLevel}: 피해 {result.damage:N0} / {result.duration:0.0}초, 처치 {result.kills}, 경로 끝 {result.leaked}{(timeout ? " (시간 초과)" : "")}");

        Store store = LoadStore();
        store.results.RemoveAll(r => r.difficulty == result.difficulty && r.wave == result.wave && r.key == result.key && r.mergeLevel == result.mergeLevel && r.noEliteBoss == result.noEliteBoss && r.noDefence == result.noDefence);
        store.results.Add(result);
        SaveStore(store);
        WriteExcel(store, result.difficulty, result.wave, result.noEliteBoss, result.noDefence);
    }

    static Store LoadStore()
    {
        try
        {
            if (File.Exists(STORE_PATH))
                return JsonUtility.FromJson<Store>(File.ReadAllText(STORE_PATH)) ?? new Store();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MinionDamage] 저장된 결과 읽기 실패: {e.Message}");
        }
        return new Store();
    }

    static void SaveStore(Store store) => File.WriteAllText(STORE_PATH, JsonUtility.ToJson(store, true));

    // 저장된 결과 중 이 난이도·wave 것으로 엑셀 다시 만들기 (창의 "엑셀 다시 만들기" 버튼도 사용)
    public static void RewriteExcel(int wave, bool noEliteBoss, bool noDefence)
    {
        string diff = GameManager.Instance != null && GameManager.Instance.stageData != null ? GameManager.Instance.stageData.key : UserDataManager.difficultyType.ToString();
        WriteExcel(LoadStore(), diff, wave, noEliteBoss, noDefence);
        AssetDatabase.Refresh();
    }

    static void WriteExcel(Store store, string difficulty, int wave, bool noEliteBoss, bool noDefence)
    {
        List<Result> results = store.results.Where(r => r.difficulty == difficulty && r.wave == wave && r.noEliteBoss == noEliteBoss && r.noDefence == noDefence).ToList();
        string path = MakePath(difficulty, wave, noEliteBoss, noDefence);
        int maxLv = MinionData.MAX_MERGE_LEVEL;

        var xlsx = new SimpleXlsxWriter();

        // 요약: 종마다 한 줄, 머지 레벨별 피해
        var widths = new List<float> { 7, 14, 12 };
        for (int lv = 0; lv <= maxLv; lv++) widths.Add(14);
        widths.Add(12);
        List<object[]> summary = xlsx.AddSheet("요약", widths.ToArray());
        var head = new List<object> { "Key", "이름" };
        for (int lv = 0; lv <= maxLv; lv++) head.Add($"머지 {lv} 피해");
        head.Add($"머지 {maxLv}/0 배율");
        summary.Add(head.ToArray());
        foreach (var g in results.GroupBy(r => r.key).OrderBy(g => GameSetting.GetMinionOrder(g.Key)).ThenBy(g => g.Key))
        {
            var row = new List<object> { g.Key, MinionName(g.Key) };
            for (int lv = 0; lv <= maxLv; lv++)
            {
                Result r = g.FirstOrDefault(x => x.mergeLevel == lv);
                row.Add(r != null ? (object)Math.Round(r.damage) : null);
            }
            Result r0 = g.FirstOrDefault(x => x.mergeLevel == 0);
            Result rMax = g.FirstOrDefault(x => x.mergeLevel == maxLv);
            row.Add(r0 != null && rMax != null && r0.damage > 0 ? (object)Math.Round(rMax.damage / r0.damage, 2) : null);
            summary.Add(row.ToArray());
        }

        // 상세: 측정 한 번에 한 줄
        List<object[]> detail = xlsx.AddSheet("상세", new float[] { 7, 14, 12, 10, 10, 10, 14, 14, 10, 10, 8, 10, 10, 8, 10, 18 });
        detail.Add(new object[] { "Key", "이름", "머지 레벨", "공격력", "공속", "피해(이 종)", "피해(전체)", "DPS", "진행 시간", "처치", "경로 끝", "시간 초과", "wave", "난이도", "측정 시각" });
        foreach (Result r in results.OrderBy(r => GameSetting.GetMinionOrder(r.key)).ThenBy(r => r.key).ThenBy(r => r.mergeLevel))
        {
            detail.Add(new object[]
            {
                r.key, MinionName(r.key), r.mergeLevel, Math.Round(r.attackPower, 2), Math.Round(r.attackSpeed, 2),
                Math.Round(r.damage), Math.Round(r.totalDamage), r.duration > 0 ? Math.Round(r.damage / r.duration, 1) : 0,
                Math.Round(r.duration, 1), r.kills, r.leaked, r.timeout ? "O" : "", r.wave, r.difficulty, r.measuredAt,
            });
        }

        try
        {
            xlsx.Save(path);
        }
        catch (IOException e)
        {
            if (!warnedFileLocked)
                Debug.LogWarning($"[MinionDamage] 엑셀 저장 실패 (엑셀에서 열려 있으면 닫아 주세요. 결과는 계속 쌓이고 다음 측정 때 다시 저장): {e.Message}");
            warnedFileLocked = true;
        }
    }


    static string MinionName(string key)
    {
        string name = TranslateManager.GetText($"Minion.{key}");
        return string.IsNullOrEmpty(name) || name.StartsWith("*") ? key : name;
    }
}

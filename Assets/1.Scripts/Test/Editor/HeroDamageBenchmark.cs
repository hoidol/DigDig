using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// (2026-11-25) 용사 피해 측정 (MinionTestWindow "용사 테스트"에서 실행, InGame Play 중, 에디터 전용)
// 미니언 없이 킹(용사) 혼자 칸 (2,2)에서 지정 wave를 처음부터 끝까지 돌려 킹이 준 피해를 잰다 (DamageMeterSystem 킹 피해)
//   구분 = 기본 공격 / 1단계 스킬 / 2단계 스킬 (HeroKingAbility.TAG_*). 3단계 공용 스킬·매의 눈 같은 버프는 기본 공격 피해에 섞여 들어감
// 용사 레벨(1~8) = 그 레벨까지의 HeroData 스탯·해방 스킬, 스킬 강화 옵션 = 1·2단계 스킬 강화 카드를 고른 상태
// 스킬은 에너지가 모이면 자동 사용 (위치 지정 스킬 = 적이 가장 몰린 곳), 에너지는 실제처럼 충전
// 결과: Library/HeroDamageTest.json 누적 → Assets/MinionDamageTest/HeroDamage_{난이도}_Wave{N}.xlsx (요약 + 상세)
// 측정 중: 킹 무적, 필드 미니언·적 정리, 킹 성장 능력은 측정마다 새로 장착 (측정 후엔 Play를 다시 시작 권장)
public static class HeroDamageBenchmark
{
    public static readonly Vector2Int TILE_INDEX = new Vector2Int(2, 2);
    const string STORE_PATH = "Library/HeroDamageTest.json";
    const int RANDOM_SEED = 1234;
    const float EXTRA_TIMEOUT = 30f;
    const float MAX_WAVE_TIME = 180f; // 킹 혼자라 적이 경로 끝까지 가도 wave는 끝남. 그래도 안 끝나면 강제 종료

    [Serializable]
    public class Result
    {
        public string difficulty;
        public int wave;
        public string hero;
        public int level;
        public bool enhanced;
        public float attackPower;
        public float attackSpeed;
        public double damage;
        public double basicDamage;
        public double skill1Damage;
        public double skill2Damage;
        public int skill1Uses, skill2Uses, skill3Uses;
        public float duration;
        public int kills;
        public int leaked;
        public bool timeout;
        public bool noEliteBoss;
        public bool noDefence;
        public string measuredAt;
    }

    [Serializable]
    class Store { public List<Result> results = new(); }

    enum State { Idle, Prepare, Running }

    static State state = State.Idle;
    static readonly Queue<AbilityData> queue = new();
    static AbilityData current;
    static int waveNum, heroLevel;
    static bool enhance, noEliteBoss, noDefence, subscribed, warnedFileLocked;
    static float prevTimeScale, startGameTime;
    static int kills, leaked, totalCount;
    static readonly int[] uses = new int[HeroKingAbility.SKILL_COUNT];

    public static bool IsRunning => state != State.Idle;
    public static string Progress => IsRunning
        ? $"{totalCount - queue.Count}/{totalCount}  {current?.title} Lv.{heroLevel}{(enhance ? " 강화" : "")}  ({(state == State.Running ? $"{GameManager.Instance.gameTimer - startGameTime:0}초" : "준비")})"
        : "";

    public static string GetOutputPath(int wave) =>
        $"{MinionDamageBenchmark.OUTPUT_FOLDER}/HeroDamage_{Difficulty()}_Wave{wave}.xlsx";

    static string Difficulty() => GameManager.Instance != null && GameManager.Instance.stageData != null
        ? GameManager.Instance.stageData.key : UserDataManager.difficultyType.ToString();

    public static void Run(IEnumerable<AbilityData> heroes, int wave, int level, bool enhanceSkills, bool withoutEliteBoss, bool ignoreDefence)
    {
        if (IsRunning)
            return;
        GameManager gm = GameManager.Instance;
        if (gm == null || !gm.IsGameStarted || !gm.isPlaying)
        {
            Debug.LogWarning("[HeroDamage] 게임이 시작된 뒤에 실행하세요");
            return;
        }
        queue.Clear();
        foreach (AbilityData h in heroes)
            if (h != null && h.prefab is HeroKingAbility)
                queue.Enqueue(h);
        totalCount = queue.Count;
        if (totalCount == 0)
            return;
        waveNum = Mathf.Clamp(wave, 1, GameSetting.LAST_WAVE);
        heroLevel = Mathf.Clamp(level, 1, GameSetting.KING_MAX_LEVEL);
        enhance = enhanceSkills;
        noEliteBoss = withoutEliteBoss;
        noDefence = ignoreDefence;
        GameManager.forceBattleWaveForTest = noEliteBoss;
        DefenceInfo.ignoreForTest = noDefence;
        prevTimeScale = Time.timeScale;
        warnedFileLocked = false;
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
        Debug.Log($"[HeroDamage] 측정 시작: {totalCount}명 (wave {waveNum}, Lv.{heroLevel}{(enhance ? ", 스킬 강화" : "")})");
    }

    public static void Stop()
    {
        if (IsRunning)
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
            Time.timeScale = prevTimeScale;
        AssetDatabase.Refresh();
        Debug.Log($"[HeroDamage] {reason} → {(GameManager.Instance != null ? GetOutputPath(waveNum) : MinionDamageBenchmark.OUTPUT_FOLDER)}");
    }

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
        king.enabled = true;
        if (king.health != null && king.health.curHp < king.MaxHp)
            king.health.curHp = king.MaxHp;
        Time.timeScale = MinionDamageBenchmark.TIME_SCALE;

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
                AutoUseSkills(king);
                float elapsed = gm.gameTimer - startGameTime;
                if (gm.isBreakTime || elapsed > MAX_WAVE_TIME + EXTRA_TIMEOUT)
                {
                    EndRun(gm, timeout: !gm.isBreakTime);
                    NextRun();
                }
                break;
        }
    }

    // 에너지가 모이면 높은 단계 스킬부터 (위치 지정 = 적이 가장 몰린 곳)
    static void AutoUseSkills(KingSlime king)
    {
        HeroKingAbility hero = king.abilityInventory.Get<HeroKingAbility>();
        if (hero == null)
            return;
        for (int idx = hero.UnlockedSkillCount - 1; idx >= 0; idx--)
        {
            if (hero.Charges < hero.GetSkillCost(idx))
                continue;
            HeroKingAbility.UseResult r;
            if (idx != HeroKingAbility.COMMON_SKILL_INDEX && hero.NeedsTargetPosition(idx))
            {
                if (!HeroKingAbility.FindDensest(Mathf.Max(1f, hero.GetTargetRadius(idx)), out Vector2 pos))
                    continue;
                r = hero.TryUseSkillAt(idx, pos);
            }
            else
                r = hero.TryUseSkill(idx);
            if (r == HeroKingAbility.UseResult.Used)
            {
                uses[idx]++;
                return;
            }
        }
    }

    // 용사 장착 = GameTestUtil.EquipHero (기기 테스트 패널·창·자동 테스트 공용)
    public static HeroKingAbility EquipHero(AbilityData heroData, int level, bool enhanceSkills) => GameTestUtil.EquipHero(heroData, level, enhanceSkills);

    static void StartRun(GameManager gm)
    {
        CleanField(gm);
        Tile tile = TileManager.Instance.GetTile(TILE_INDEX);
        if (tile == null)
        {
            Finish($"칸 {TILE_INDEX} 없음");
            return;
        }
        if (tile.isBlocked)
            tile.Unblock();
        KingSlime king = KingSlime.Instance;
        if (!ReferenceEquals(tile.tileObject, king))
        {
            if (tile.tileObject != null)
            {
                Finish($"칸 {TILE_INDEX}이 비어 있지 않음");
                return;
            }
            TileManager.Instance.RemoveTileObject(king);
            tile.SetTileObject(king);
            king.transform.position = tile.transform.position;
        }

        UnityEngine.Random.InitState(RANDOM_SEED);
        HeroKingAbility hero = EquipHero(current, heroLevel, enhance);
        if (hero == null)
        {
            Finish($"{current.key} 용사 장착 실패");
            return;
        }
        Array.Clear(uses, 0, uses.Length);
        DamageMeterSystem.Clear();
        kills = leaked = 0;
        startGameTime = gm.gameTimer;
        gm.isBreakTime = true;
        gm.breakTimeNextWaveIdx = waveNum - 1;
        gm.EndBreakTime();
        state = State.Running;
    }

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
        KingSlime king = KingSlime.Instance;
        var result = new Result
        {
            difficulty = gm.stageData != null ? gm.stageData.key : "",
            wave = waveNum,
            hero = current.key,
            level = heroLevel,
            enhanced = enhance,
            attackPower = king.statMgr.AttackPower,
            attackSpeed = king.statMgr.AttackSpeed,
            damage = DamageMeterSystem.KingTotalDamage,
            basicDamage = DamageMeterSystem.GetKingDamage(HeroKingAbility.TAG_BASIC),
            skill1Damage = DamageMeterSystem.GetKingDamage(HeroKingAbility.TAG_SKILL1),
            skill2Damage = DamageMeterSystem.GetKingDamage(HeroKingAbility.TAG_SKILL2),
            skill1Uses = uses[0], skill2Uses = uses[1], skill3Uses = uses[2],
            duration = gm.gameTimer - startGameTime,
            kills = kills,
            leaked = leaked,
            timeout = timeout,
            noEliteBoss = noEliteBoss,
            noDefence = noDefence,
            measuredAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
        };
        if (timeout)
        {
            Wave wave = gm.CurrentWave;
            if (wave != null && !wave.isEnd)
                wave.EndWave();
        }
        Debug.Log($"[HeroDamage] {current.title} Lv.{heroLevel}{(enhance ? " 강화" : "")}: 피해 {result.damage:N0} (기본 {result.basicDamage:N0} / 스킬1 {result.skill1Damage:N0} / 스킬2 {result.skill2Damage:N0}) / {result.duration:0.0}초, 처치 {kills}, 경로 끝 {leaked}");

        Store store = LoadStore();
        store.results.RemoveAll(r => r.difficulty == result.difficulty && r.wave == result.wave && r.hero == result.hero && r.level == result.level
                                     && r.enhanced == result.enhanced && r.noEliteBoss == result.noEliteBoss && r.noDefence == result.noDefence);
        store.results.Add(result);
        File.WriteAllText(STORE_PATH, JsonUtility.ToJson(store, true));
        WriteExcel(store, result.difficulty, result.wave);
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
            Debug.LogWarning($"[HeroDamage] 저장된 결과 읽기 실패: {e.Message}");
        }
        return new Store();
    }

    public static void RewriteExcel(int wave)
    {
        WriteExcel(LoadStore(), Difficulty(), wave);
        AssetDatabase.Refresh();
    }

    static void WriteExcel(Store store, string difficulty, int wave)
    {
        List<Result> results = store.results.Where(r => r.difficulty == difficulty && r.wave == wave)
            .OrderBy(r => r.hero).ThenBy(r => r.level).ThenBy(r => r.enhanced).ToList();
        var xlsx = new SimpleXlsxWriter();
        List<object[]> sheet = xlsx.AddSheet("용사 피해", new float[] { 14, 8, 8, 10, 10, 14, 14, 14, 14, 10, 8, 8, 8, 10, 8, 8, 10, 12, 12, 18 });
        sheet.Add(new object[] { "용사", "레벨", "스킬 강화", "공격력", "공속", "총 피해", "기본 공격", "1단계 스킬", "2단계 스킬", "DPS", "1단계 사용", "2단계 사용", "3단계 사용",
            "진행 시간", "처치", "경로 끝", "시간 초과", "엘리트·보스 없이", "상성 무시", "측정 시각" });
        foreach (Result r in results)
            sheet.Add(new object[]
            {
                HeroName(r.hero), r.level, r.enhanced ? "O" : "", Math.Round(r.attackPower, 2), Math.Round(r.attackSpeed, 2),
                Math.Round(r.damage), Math.Round(r.basicDamage), Math.Round(r.skill1Damage), Math.Round(r.skill2Damage),
                r.duration > 0 ? Math.Round(r.damage / r.duration, 1) : 0, r.skill1Uses, r.skill2Uses, r.skill3Uses,
                Math.Round(r.duration, 1), r.kills, r.leaked, r.timeout ? "O" : "", r.noEliteBoss ? "O" : "", r.noDefence ? "O" : "", r.measuredAt,
            });
        try
        {
            xlsx.Save(GetOutputPathFor(difficulty, wave));
        }
        catch (IOException e)
        {
            if (!warnedFileLocked)
                Debug.LogWarning($"[HeroDamage] 엑셀 저장 실패 (엑셀에서 열려 있으면 닫아 주세요): {e.Message}");
            warnedFileLocked = true;
        }
    }

    static string GetOutputPathFor(string difficulty, int wave) => $"{MinionDamageBenchmark.OUTPUT_FOLDER}/HeroDamage_{difficulty}_Wave{wave}.xlsx";

    static string HeroName(string key)
    {
        AbilityData d = KingGrowthSetting.Instance != null ? KingGrowthSetting.Instance.heroAbilities.FirstOrDefault(h => h != null && h.key == key) : null;
        return d != null && !string.IsNullOrEmpty(d.title) ? d.title : key;
    }
}

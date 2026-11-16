using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// 미니언 수동 테스트 창 (Tools/Test/Minion Test). InGame 씬 Play 중에 사용, 에디터 전용 (빌드에 안 들어감)
// TestCanvas(배속·골드·다음 wave)와 별개: 미니언 소환·스킬·적·타일 아이템을 바로 조작
// 고른 값은 EditorPrefs에 저장 → Play를 다시 눌러도 유지
public class MinionTestWindow : EditorWindow
{
    const string PREFS = "MinionTestWindow.";

    // 소환
    string minionKey;
    int mergeLevel;
    bool evolve;
    bool useTileIndex;
    Vector2Int tileIndex;

    // 편의
    bool invincible;
    bool infiniteCoin;

    // 적
    int targetWave = 1; // 표시 번호 (waveIdx + 1)
    EnemyType enemyType = EnemyType.Normal;
    DefenceType defenceType = DefenceType.Normal;
    int enemyCount = 1;
    bool freezeEnemies;

    // 타일 아이템
    string itemKey;

    Vector2 scroll;

    [MenuItem("Tools/Test/Minion Test")]
    static void Open() => GetWindow<MinionTestWindow>("Minion Test");

    void OnEnable()
    {
        minionKey = EditorPrefs.GetString(PREFS + "minionKey", "");
        mergeLevel = EditorPrefs.GetInt(PREFS + "mergeLevel", 0);
        evolve = EditorPrefs.GetBool(PREFS + "evolve", false);
        useTileIndex = EditorPrefs.GetBool(PREFS + "useTileIndex", false);
        tileIndex = new Vector2Int(EditorPrefs.GetInt(PREFS + "tileX", 0), EditorPrefs.GetInt(PREFS + "tileY", 0));
        invincible = EditorPrefs.GetBool(PREFS + "invincible", true);
        infiniteCoin = EditorPrefs.GetBool(PREFS + "infiniteCoin", true);
        targetWave = EditorPrefs.GetInt(PREFS + "targetWave", 1);
        enemyType = (EnemyType)EditorPrefs.GetInt(PREFS + "enemyType", 0);
        defenceType = (DefenceType)EditorPrefs.GetInt(PREFS + "defenceType", 0);
        enemyCount = EditorPrefs.GetInt(PREFS + "enemyCount", 1);
        itemKey = EditorPrefs.GetString(PREFS + "itemKey", "");
        EditorApplication.update += OnEditorUpdate;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        Save();
    }

    void Save()
    {
        EditorPrefs.SetString(PREFS + "minionKey", minionKey ?? "");
        EditorPrefs.SetInt(PREFS + "mergeLevel", mergeLevel);
        EditorPrefs.SetBool(PREFS + "evolve", evolve);
        EditorPrefs.SetBool(PREFS + "useTileIndex", useTileIndex);
        EditorPrefs.SetInt(PREFS + "tileX", tileIndex.x);
        EditorPrefs.SetInt(PREFS + "tileY", tileIndex.y);
        EditorPrefs.SetBool(PREFS + "invincible", invincible);
        EditorPrefs.SetBool(PREFS + "infiniteCoin", infiniteCoin);
        EditorPrefs.SetInt(PREFS + "targetWave", targetWave);
        EditorPrefs.SetInt(PREFS + "enemyType", (int)enemyType);
        EditorPrefs.SetInt(PREFS + "defenceType", (int)defenceType);
        EditorPrefs.SetInt(PREFS + "enemyCount", enemyCount);
        EditorPrefs.SetString(PREFS + "itemKey", itemKey ?? "");
    }

    void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
            freezeEnemies = false; // static 값이라 다음 Play에 남지 않게
    }

    static bool InGameReady => Application.isPlaying && KingSlime.Instance != null && GameManager.Instance != null
                               && MinionFieldController.Instance != null && TileManager.Instance != null;

    // 무적·코인 무한은 매 프레임 유지 (사망 시 EndGame에서 유저 데이터가 저장되므로 무적 기본 켬)
    void OnEditorUpdate()
    {
        if (!InGameReady)
            return;
        KingSlime king = KingSlime.Instance;
        if (invincible && king.health != null && king.health.curHp < king.MaxHp)
            king.health.curHp = king.MaxHp;
        if (infiniteCoin)
        {
            if (king.coin < 100000)
                king.AddCoin(100000 - king.coin);
            if (king.diceCoin < 1000)
                king.AddDiceCoin(1000 - king.diceCoin);
        }
        if (freezeEnemies)
            Enemy.FreezeAll(1f);
        if (Application.isPlaying)
            Repaint(); // 선택 미니언 상태 갱신
    }

    void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("InGame 씬을 Play하면 사용할 수 있습니다.\n튜토리얼은 BuildSetting.skipTutorial을 켜 두세요.", MessageType.Info);
            return;
        }
        if (!InGameReady)
        {
            EditorGUILayout.HelpBox("InGame 씬이 준비되지 않았습니다.", MessageType.Warning);
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUI.BeginChangeCheck();
        DrawSpawn();
        DrawSelected();
        DrawConvenience();
        DrawEnemy();
        DrawTileItem();
        if (EditorGUI.EndChangeCheck())
            Save();
        EditorGUILayout.EndScrollView();
    }

    // ---------------- 소환 ----------------
    void DrawSpawn()
    {
        Header("미니언 소환");
        string[] keys = MinionManager.Instance != null
            ? MinionManager.Instance.minionDataDic.Values.OrderBy(d => d.grade).ThenBy(d => d.key).Select(d => d.key).ToArray()
            : new string[0];
        if (keys.Length == 0)
        {
            EditorGUILayout.HelpBox("미니언 데이터가 아직 없습니다.", MessageType.None);
            return;
        }
        string[] labels = keys.Select(k => $"{MinionManager.Instance.minionDataDic[k].grade} / {k}").ToArray();
        int idx = Mathf.Max(0, System.Array.IndexOf(keys, minionKey));
        idx = EditorGUILayout.Popup("Key", idx, labels);
        minionKey = keys[idx];

        mergeLevel = EditorGUILayout.IntSlider("머지 레벨 (표시 = +1)", mergeLevel, 0, MinionData.MAX_MERGE_LEVEL);
        evolve = EditorGUILayout.Toggle("진화 (최대 레벨만)", evolve);
        useTileIndex = EditorGUILayout.Toggle("칸 지정", useTileIndex);
        if (useTileIndex)
            tileIndex = EditorGUILayout.Vector2IntField("칸 (x 왼→오, y 위→아래)", tileIndex);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("소환"))
                Spawn(minionKey);
            if (GUILayout.Button("같은 종 2마리 (합성 확인)"))
            {
                bool prev = useTileIndex;
                useTileIndex = false;
                Spawn(minionKey);
                Spawn(minionKey);
                useTileIndex = prev;
            }
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("빈 칸 모두 채우기 (이 종)"))
                while (FindTile() != null && Spawn(minionKey) != null) { }
            if (GUILayout.Button("필드 미니언 전부 제거"))
                ClearMinions();
        }
    }

    Tile FindTile()
    {
        if (useTileIndex)
        {
            Tile t = TileManager.Instance.GetTile(tileIndex);
            if (t != null && t.isBlocked)
                t.Unblock(); // 테스트용: 막힌 칸이면 풀어서 사용
            return t != null && t.tileObject == null ? t : null;
        }
        return TileManager.Instance.tiles.FirstOrDefault(t => t.IsEmpty);
    }

    Minion Spawn(string key)
    {
        Tile tile = FindTile();
        if (tile == null)
        {
            Debug.LogWarning(useTileIndex ? $"[MinionTest] {tileIndex} 칸에 이미 유닛이 있습니다" : "[MinionTest] 빈 칸이 없습니다");
            return null;
        }
        Minion minion = KingSlime.Instance.AddMinion(key, mergeLevel);
        tile.SetTileObject(minion);
        if (evolve && minion.CanEvolve && mergeLevel >= MinionData.MAX_MERGE_LEVEL)
            minion.Evolve();
        return minion;
    }

    static void ClearMinions()
    {
        foreach (Minion m in MinionFieldController.Instance.activeminions.ToList())
        {
            if (m != null && TileManager.Instance.GetTile(m) != null)
                MinionFieldController.Instance.RemoveFieldMinion(m);
        }
    }

    // ---------------- 선택한 미니언 ----------------
    void DrawSelected()
    {
        Header("선택한 미니언 (필드에서 클릭)");
        Tile tile = MinionFieldController.Instance.SelectedTile;
        Minion m = tile != null ? tile.tileObject as Minion : null;
        if (m == null)
        {
            EditorGUILayout.LabelField("선택된 미니언 없음");
            return;
        }

        EditorGUILayout.LabelField($"{m.key}  머지 {m.mergeLevel + 1}  {(m.isEvolved ? "[진화]" : "")}  칸 {tile.index}");
        EditorGUILayout.LabelField($"공격력 {m.statDic[StatType.AttackPower]:0.##}   공속 {m.AttackSpeed:0.##}   사거리 {m.AttackRange:0.##}   치명 {m.CritChance:0.##}%");
        if (m.minionData != null && m.minionData.hasSkill)
            EditorGUILayout.LabelField($"스킬 진행도 {m.SkillProgress * 100f:0}%" + (m.StateText != null ? $"   {m.StateText}" : ""));
        if (m.activeBuffs.Count > 0)
            EditorGUILayout.LabelField("버프: " + string.Join(", ", m.activeBuffs.Select(b => $"{b.statType} {b.opType} {b.value:0.##}")));

        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.enabled = m.minionData != null && m.minionData.hasSkill;
            if (GUILayout.Button("스킬 쿨타임 채우기"))
                m.FillSkillForTest();
            GUI.enabled = m.CanEvolve && !m.isEvolved && m.mergeLevel >= MinionData.MAX_MERGE_LEVEL;
            if (GUILayout.Button("진화"))
                m.Evolve();
            GUI.enabled = m.mergeLevel < MinionData.MAX_MERGE_LEVEL;
            if (GUILayout.Button("머지 +1"))
                MinionFieldController.Instance.ReplaceMinion(m, m.key, m.mergeLevel + 1);
            GUI.enabled = true;
            if (GUILayout.Button("제거"))
                MinionFieldController.Instance.RemoveFieldMinion(m);
        }
        EditorGUILayout.HelpBox("쿨타임 채우기: 시간형은 바로 참, 처치·공격 횟수형은 1회 남김(다음 처치·공격에 발동)", MessageType.None);
    }

    // ---------------- 편의 ----------------
    void DrawConvenience()
    {
        Header("편의 (배속·골드는 TestCanvas에도 있음)");
        invincible = EditorGUILayout.Toggle("킹 무적", invincible);
        infiniteCoin = EditorGUILayout.Toggle("코인·주사위 코인 무한", infiniteCoin);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField($"속도 x{Time.timeScale:0.##}", GUILayout.Width(80));
            foreach (float s in new[] { 0f, 0.25f, 0.5f, 1f, 2f, 4f })
            {
                if (GUILayout.Button(s == 0 ? "정지" : $"x{s}"))
                    Time.timeScale = s;
            }
        }
    }

    // ---------------- 적 ----------------
    void DrawEnemy()
    {
        Header("적");
        GameManager gm = GameManager.Instance;
        EditorGUILayout.LabelField($"현재 wave {gm.waveIdx + 1} ({GameManager.GetWaveType(gm.waveIdx)}, {GameManager.GetWaveDefenceType(gm.waveIdx)})   필드 적 {EnemyFieldController.Instance.ActiveEnemyCount}");
        using (new EditorGUILayout.HorizontalScope())
        {
            targetWave = Mathf.Max(1, EditorGUILayout.IntField("wave 번호", targetWave));
            if (GUILayout.Button("이 wave 시작", GUILayout.Width(100)))
            {
                if (!gm.isPlaying)
                    Debug.LogWarning("[MinionTest] 게임이 아직 시작 전입니다");
                else
                    gm.StartWave(targetWave - 1);
            }
        }

        enemyType = (EnemyType)EditorGUILayout.EnumPopup("종류", enemyType);
        if (enemyType == EnemyType.Normal)
            defenceType = (DefenceType)EditorGUILayout.EnumPopup("방어 타입", defenceType);
        enemyCount = EditorGUILayout.IntSlider("수", enemyCount, 1, 20);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("적 소환 (현재 wave 체력)"))
                SpawnEnemies();
            if (GUILayout.Button("필드 적 전부 제거"))
                EnemyFieldController.Instance.ClearAllEnemies();
        }
        bool freeze = EditorGUILayout.Toggle("적 정지 (허수아비)", freezeEnemies);
        if (freeze != freezeEnemies)
        {
            freezeEnemies = freeze;
            if (!freeze)
                Enemy.ClearFreezeAll();
        }
    }

    void SpawnEnemies()
    {
        StageData stage = GameManager.Instance.stageData;
        EnemyFieldController field = EnemyFieldController.Instance;
        DefenceType def = enemyType == EnemyType.Normal ? defenceType : DefenceType.Normal; // 엘리트·보스는 항상 Normal
        Enemy prefab = stage != null ? stage.GetEnemyPrefab(enemyType, def) : null;
        if (prefab == null)
        {
            Debug.LogWarning($"[MinionTest] 적 프리팹 없음: {enemyType} / {def}");
            return;
        }
        int wave = GameManager.Instance.waveIdx;
        for (int i = 0; i < enemyCount; i++)
        {
            Enemy enemy = field.Instantiate(prefab);
            if (enemy == null)
                continue;
            if (enemyType == EnemyType.Normal)
            {
                enemy.Spawn(field.GetNormalSpawnPosition(), wave);
                enemy.pathIdx = field.NormalStartPathIdx;
            }
            else
                enemy.Spawn(field.GetSpawnPosition(), wave);
        }
    }

    // ---------------- 타일 아이템 ----------------
    void DrawTileItem()
    {
        Header("타일 아이템 (선택한 칸에 설치)");
        if (ItemManager.Instance == null)
            return;
        string[] keys = ItemManager.Instance.itemDataDic.Keys.OrderBy(k => k).ToArray();
        if (keys.Length == 0)
            return;
        int idx = Mathf.Max(0, System.Array.IndexOf(keys, itemKey));
        idx = EditorGUILayout.Popup("Key", idx, keys);
        itemKey = keys[idx];

        Tile tile = MinionFieldController.Instance.SelectedTile;
        TileItem cur = tile != null ? tile.tileItem as TileItem : null;
        EditorGUILayout.LabelField(tile == null ? "선택한 칸 없음 (빈 칸은 선택이 안 되므로 유닛이 있는 칸을 클릭)"
            : cur == null ? $"칸 {tile.index}: 아이템 없음" : $"칸 {tile.index}: {cur.key} Lv.{cur.upgradeLevel}");

        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.enabled = tile != null && tile.CanAddTileItem;
            if (GUILayout.Button("설치"))
                KingSlime.Instance.AddItem(itemKey, tile).Forget();
            GUI.enabled = cur != null && cur.CanUpgrade && !cur.IsMaxLevel;
            if (GUILayout.Button("레벨 +1 (확정)"))
                cur.TryUpgrade(0f);
            GUI.enabled = cur != null;
            if (GUILayout.Button("제거"))
                KingSlime.Instance.RemoveItem(cur);
            GUI.enabled = true;
        }
    }

    static void Header(string title)
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
    }
}

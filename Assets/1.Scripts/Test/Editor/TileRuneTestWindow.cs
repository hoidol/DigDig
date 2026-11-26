using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEditor;
using UnityEngine;

// 타일 아이템 수동 테스트 창 (Tools/Test/TileRune Test). InGame 씬 Play 중에 사용, 에디터 전용 (빌드에 안 들어감)
// 6*6 칸 판에서 칸을 골라(빈 칸·막힌 칸도 가능) 아이템을 원하는 레벨로 설치·강화·제거하고,
// 그 칸에 미니언을 올려 효과(스탯·발사)를 바로 확인한다. 고른 값은 EditorPrefs에 저장
public class TileRuneTestWindow : EditorWindow
{
    const string PREFS = "TileItemTestWindow.";
    const int GRID = 6;

    string runeKey;
    int installLevel = 1;
    Vector2Int target;       // 고른 칸 (x 왼→오, y 위→아래)

    // 칸에 올릴 미니언
    string minionKey;
    int mergeLevel;

    // 편의
    bool invincible;
    bool infiniteCoin;
    bool freezeEnemies;
    int enemyCount = 5;
    DefenceType defenceType = DefenceType.Normal;

    Vector2 scroll;

    [MenuItem("Tools/Test/Rune Test")]
    static void Open() => GetWindow<TileRuneTestWindow>("Rune Test");

    void OnEnable()
    {
        runeKey = EditorPrefs.GetString(PREFS + "itemKey", "");
        installLevel = EditorPrefs.GetInt(PREFS + "installLevel", 1);
        target = new Vector2Int(EditorPrefs.GetInt(PREFS + "targetX", 0), EditorPrefs.GetInt(PREFS + "targetY", 0));
        minionKey = EditorPrefs.GetString(PREFS + "minionKey", "");
        mergeLevel = EditorPrefs.GetInt(PREFS + "mergeLevel", 0);
        invincible = EditorPrefs.GetBool(PREFS + "invincible", true);
        infiniteCoin = EditorPrefs.GetBool(PREFS + "infiniteCoin", true);
        enemyCount = EditorPrefs.GetInt(PREFS + "enemyCount", 5);
        defenceType = (DefenceType)EditorPrefs.GetInt(PREFS + "defenceType", 0);
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
        EditorPrefs.SetString(PREFS + "itemKey", runeKey ?? "");
        EditorPrefs.SetInt(PREFS + "installLevel", installLevel);
        EditorPrefs.SetInt(PREFS + "targetX", target.x);
        EditorPrefs.SetInt(PREFS + "targetY", target.y);
        EditorPrefs.SetString(PREFS + "minionKey", minionKey ?? "");
        EditorPrefs.SetInt(PREFS + "mergeLevel", mergeLevel);
        EditorPrefs.SetBool(PREFS + "invincible", invincible);
        EditorPrefs.SetBool(PREFS + "infiniteCoin", infiniteCoin);
        EditorPrefs.SetInt(PREFS + "enemyCount", enemyCount);
        EditorPrefs.SetInt(PREFS + "defenceType", (int)defenceType);
    }

    void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
            freezeEnemies = false;
    }

    static bool InGameReady => Application.isPlaying && KingSlime.Instance != null && GameManager.Instance != null
                               && MinionFieldController.Instance != null && TileManager.Instance != null && RuneManager.Instance != null;

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
            // 주사위 코인은 채우지 않음 (필요하면 직접 추가)
        }
        if (freezeEnemies)
            Enemy.FreezeAll(1f);
        Repaint();
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
        DrawRuneSelect();
        DrawBoard();
        DrawTarget();
        DrawInstalledList();
        DrawDraw();
        DrawConvenience();
        if (EditorGUI.EndChangeCheck())
            Save();
        EditorGUILayout.EndScrollView();
    }

    // ---------------- 아이템 고르기 ----------------
    RuneData SelectedData => RuneData.GetRuneData(runeKey);

    void DrawRuneSelect()
    {
        Header("아이템");
        string[] keys = RuneManager.Instance.runeDataDic.Values
            .Where(d => d != null && d.runePrefab is TileRune)
            .OrderBy(d => d.key).Select(d => d.key).ToArray();
        if (keys.Length == 0)
        {
            EditorGUILayout.HelpBox("타일 아이템 데이터가 없습니다.", MessageType.None);
            return;
        }
        string[] labels = keys.Select(k => $"{k} ({RuneManager.Instance.runeDataDic[k].runeName})").ToArray();
        int idx = Mathf.Max(0, System.Array.IndexOf(keys, runeKey));
        idx = EditorGUILayout.Popup("Key", idx, labels);
        runeKey = keys[idx];

        RuneData data = SelectedData;
        TileRune prefab = data.runePrefab as TileRune;
        installLevel = data.CanUpgradeRune ? EditorGUILayout.IntSlider("설치 레벨", installLevel, 1, data.MaxLevel) : 1;

        string values = data.HasLevelValues ? string.Join(" / ", data.levelValues.Select(v => v.ToString("0.##"))) : "없음 (프리팹 값)";
        EditorGUILayout.LabelField($"레벨 수치: {values}   최대 Lv {data.MaxLevel}   주사위 {data.diceCoin}" + (data.CanUpgradeRune ? "" : "   (강화 불가)"));
        string kind = prefab is PioneerTileRune ? "막힌 칸을 골라 해제 (설치 안 함)"
            : prefab != null && !prefab.NeedTile ? "획득 즉시 적용 (칸 필요 없음)"
            : "칸에 설치";
        EditorGUILayout.LabelField("방식: " + kind);
        EditorGUILayout.HelpBox(data.desc, MessageType.None);
    }

    // ---------------- 칸 판 ----------------
    void DrawBoard()
    {
        Header("칸 (눌러서 고르기)  ■ 막힘 / 위 = 유닛, 아래 = 아이템");
        GUIStyle style = new GUIStyle(GUI.skin.button) { wordWrap = true, fontSize = 9, alignment = TextAnchor.MiddleCenter };
        Color prevColor = GUI.backgroundColor;
        float size = Mathf.Clamp((position.width - 40) / GRID, 40, 90);
        for (int y = 0; y < GRID; y++)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                for (int x = 0; x < GRID; x++)
                {
                    Vector2Int index = new Vector2Int(x, y);
                    Tile tile = TileManager.Instance.GetTile(index);
                    GUI.backgroundColor = index == target ? Color.cyan
                        : tile == null || tile.isBlocked ? new Color(0.45f, 0.45f, 0.45f)
                        : tile.tileRune != null ? new Color(1f, 0.85f, 0.4f)
                        : Color.white;
                    if (GUILayout.Button(TileLabel(tile), style, GUILayout.Width(size), GUILayout.Height(size * 0.6f)))
                        target = index;
                }
            }
        }
        GUI.backgroundColor = prevColor;
    }

    static string TileLabel(Tile tile)
    {
        if (tile == null)
            return "-";
        string unit = tile.isBlocked ? "■"
            : tile.tileObject is Minion m ? $"{m.key} {m.mergeLevel + 1}"
            : tile.tileObject is KingSlime ? "킹"
            : "";
        string rune = tile.tileRune is TileRune ti ? $"{ti.key.Replace("Tile", "")} Lv{ti.upgradeLevel}" : "";
        return $"{unit}\n{rune}";
    }

    // ---------------- 고른 칸 ----------------
    void DrawTarget()
    {
        Tile tile = TileManager.Instance.GetTile(target);
        Header($"고른 칸 {target}");
        if (tile == null)
            return;

        TileRune cur = tile.tileRune as TileRune;
        RuneData data = SelectedData;
        TileRune prefab = data != null ? data.runePrefab as TileRune : null;

        // 상태
        if (tile.isBlocked)
            EditorGUILayout.LabelField("막힌 칸");
        if (cur != null)
        {
            string lvValue = cur.runeData != null && cur.runeData.HasLevelValues ? $"  수치 {cur.runeData.GetLevelValue(cur.upgradeLevel):0.##}" : "";
            EditorGUILayout.LabelField($"아이템: {cur.key} Lv.{cur.upgradeLevel}{lvValue}" + (cur.CanUpgrade ? $"  (다음 강화 {cur.UpgradeChance:0}% / 주사위 {cur.UpgradeCost})" : ""));
            EditorGUILayout.HelpBox(cur.GetDescription(), MessageType.None);
        }
        else
            EditorGUILayout.LabelField("아이템 없음");
        DrawUnitInfo(tile);

        // 아이템 조작
        using (new EditorGUILayout.HorizontalScope())
        {
            if (prefab is PioneerTileRune)
            {
                GUI.enabled = tile.isBlocked;
                if (GUILayout.Button("이 칸 개척"))
                    Install(runeKey, tile, 1);
            }
            else if (prefab != null && !prefab.NeedTile)
            {
                if (GUILayout.Button("획득 (칸 없이 적용)"))
                    Install(runeKey, null, 1);
            }
            else
            {
                GUI.enabled = tile.CanAddTileRune;
                if (GUILayout.Button($"설치 (Lv.{installLevel})"))
                    Install(runeKey, tile, installLevel);
                GUI.enabled = cur != null && !tile.isBlocked;
                if (GUILayout.Button($"교체 (Lv.{installLevel})"))
                {
                    KingSlime.Instance.RemoveRune(cur);
                    Install(runeKey, tile, installLevel);
                }
            }
            GUI.enabled = true;
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.enabled = cur != null && cur.CanUpgrade && !cur.IsMaxLevel;
            if (GUILayout.Button("레벨 +1 (확정)"))
                cur.TryUpgrade(0f);
            GUI.enabled = cur != null;
            if (GUILayout.Button("아이템 제거"))
                KingSlime.Instance.RemoveRune(cur);
            GUI.enabled = tile.isBlocked;
            if (GUILayout.Button("막힘 풀기"))
                TileManager.Instance.UnblockTile(tile);
            GUI.enabled = true;
        }

        // 칸에 미니언 올리기
        EditorGUILayout.Space(4);
        DrawMinionPlace(tile);
    }

    static void DrawUnitInfo(Tile tile)
    {
        if (tile.tileObject is Minion m)
        {
            EditorGUILayout.LabelField($"미니언: {m.key} 머지 {m.mergeLevel + 1}" + (m.isEvolved ? " [진화]" : ""));
            EditorGUILayout.LabelField($"공격력 {m.statDic[StatType.AttackPower]:0.##}   공속 {m.AttackSpeed:0.##}   사거리 {m.AttackRange:0.##}   치명 {m.CritChance:0.##}%   탄 수 {m.ShootCount + m.ShotModifier.bonusShootCount}");
            if (m.activeBuffs.Count > 0)
                EditorGUILayout.LabelField("버프: " + string.Join(", ", m.activeBuffs.Select(b => $"{b.statType} {b.opType} {b.value:0.##}")));
        }
        else if (tile.tileObject is KingSlime)
            EditorGUILayout.LabelField("킹슬라임");
        else
            EditorGUILayout.LabelField("유닛 없음");
    }

    void DrawMinionPlace(Tile tile)
    {
        string[] keys = MinionManager.Instance != null
            ? MinionManager.Instance.minionDataDic.Values.OrderBy(d => GameSetting.GetMinionOrder(d.key)).ThenBy(d => d.key).Select(d => d.key).ToArray()
            : new string[0];
        if (keys.Length == 0)
            return;
        using (new EditorGUILayout.HorizontalScope())
        {
            string[] labels = keys.Select(k => $"{k} ({MinionManager.Instance.minionDataDic[k].Title})").ToArray();
            int idx = Mathf.Max(0, System.Array.IndexOf(keys, minionKey));
            idx = EditorGUILayout.Popup(idx, labels);
            minionKey = keys[idx];
            mergeLevel = EditorGUILayout.IntSlider(mergeLevel, 0, MinionData.MAX_MERGE_LEVEL, GUILayout.Width(140));
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.enabled = tile.IsEmpty;
            if (GUILayout.Button($"이 칸에 미니언 (머지 {mergeLevel + 1})"))
            {
                Minion minion = KingSlime.Instance.AddMinion(minionKey, mergeLevel);
                tile.SetTileObject(minion);
            }
            GUI.enabled = tile.tileObject is Minion;
            if (GUILayout.Button("미니언 제거"))
                MinionFieldController.Instance.RemoveFieldMinion((Minion)tile.tileObject);
            GUI.enabled = true;
        }
    }

    // 설치 후 원하는 레벨까지 확정 강화 (AddItem은 안에서 바로 설치까지 끝남)
    static void Install(string key, Tile tile, int level)
    {
        KingSlime.Instance.AddRune(key, tile).Forget();
        if (tile != null && tile.tileRune is TileRune installed)
            while (installed.upgradeLevel < level && installed.TryUpgrade(0f)) { }
    }

    // ---------------- 설치된 아이템 ----------------
    void DrawInstalledList()
    {
        TileRune[] runes = KingSlime.Instance.runeInventory.curRunes.OfType<TileRune>().ToArray();
        Header($"보유 타일 아이템 {runes.Length}개");
        foreach (TileRune rune in runes)
        {
            if (rune == null)
                continue;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"{rune.key} Lv.{rune.upgradeLevel}" + (rune.tile != null ? $"  칸 {rune.tile.index}" : "  (칸 없음)"));
                GUI.enabled = rune.tile != null;
                if (GUILayout.Button("고르기", GUILayout.Width(60)))
                    target = rune.tile.index;
                GUI.enabled = true;
                if (GUILayout.Button("제거", GUILayout.Width(50)))
                    KingSlime.Instance.RemoveRune(rune);
            }
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button($"빈 아이템 칸 전부에 설치 (Lv.{installLevel})"))
            {
                TileRune prefab = SelectedData?.runePrefab as TileRune;
                if (prefab == null || !prefab.NeedTile || prefab is PioneerTileRune)
                    Debug.LogWarning("[RuneTest] 칸에 설치하는 아이템만 가능합니다");
                else
                    foreach (Tile t in TileManager.Instance.tiles.Where(t => t.CanAddTileRune).ToList())
                        Install(runeKey, t, installLevel);
            }
            GUI.enabled = runes.Length > 0;
            if (GUILayout.Button("전부 제거"))
                foreach (TileRune rune in runes)
                    if (rune != null)
                        KingSlime.Instance.RemoveRune(rune);
            GUI.enabled = true;
        }
    }

    // ---------------- 뽑기 ----------------
    void DrawDraw()
    {
        Header("뽑기");
        EditorGUILayout.LabelField($"뽑기 후보 {RuneManager.Instance.GetDrawTileRunes(99).Count}종 (룬 뽑기 버튼은 2026-11-20 삭제)");
        if (GUILayout.Button("룬 선택 창 열기"))
            SelectRuneCanvas.Instance.OpenCanvas();
    }

    // ---------------- 편의 ----------------
    void DrawConvenience()
    {
        Header("편의 (Minion Test 창과 같은 기능)");
        invincible = EditorGUILayout.Toggle("킹 무적", invincible);
        infiniteCoin = EditorGUILayout.Toggle("코인·주사위 코인 무한", infiniteCoin);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField($"속도 x{Time.timeScale:0.##}", GUILayout.Width(80));
            foreach (float s in new[] { 0f, 0.25f, 0.5f, 1f, 2f, 4f })
                if (GUILayout.Button(s == 0 ? "정지" : $"x{s}"))
                    Time.timeScale = s;
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            defenceType = (DefenceType)EditorGUILayout.EnumPopup(defenceType, GUILayout.Width(100));
            enemyCount = EditorGUILayout.IntSlider(enemyCount, 1, 20);
            if (GUILayout.Button("일반 적 소환", GUILayout.Width(90)))
                MinionTestWindow.SpawnEnemies(EnemyType.Normal, defenceType, enemyCount);
            if (GUILayout.Button("엘리트", GUILayout.Width(55)))
                MinionTestWindow.SpawnEnemies(EnemyType.Elite, DefenceType.Normal, 1);
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            bool freeze = EditorGUILayout.Toggle("적 정지 (허수아비)", freezeEnemies);
            if (freeze != freezeEnemies)
            {
                freezeEnemies = freeze;
                if (!freeze)
                    Enemy.ClearFreezeAll();
            }
            if (GUILayout.Button("필드 적 전부 제거"))
                EnemyFieldController.Instance.ClearAllEnemies();
        }
    }

    static void Header(string title)
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
    }
}

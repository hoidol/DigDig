using UnityEngine.Serialization;
using UnityEngine;



[CreateAssetMenu]
public class RuneData : ScriptableObject
{
    public string key;
    public string Title => runeName;
    [FormerlySerializedAs("itemName")] public string runeName;
    public string desc;
    // public int consumeHp;
    public int level;
    // public int addPrice; //추가 금액
    public int maxCount = 2; //최대 보유 개수, 타일 아이템은 최대 강화 레벨도 겸함

    // 타일 아이템 강화 (CLAUDE.md "타일 아이템 강화"): 비용 = diceCoin × (현재 레벨 + 1), 0이면 강화 불가
    public int diceCoin;
    public float[] levelValues; // RuneData.csv Values ("1.3/1.5/1.7"), index = 레벨 - 1
    public int MaxLevel => maxCount;
    // (2026-11-26) 강화 = 같은 룬 칸에 다시 새기기(레벨업·룬 보상). 코인 강화 버튼이 없어져 DiceCoin 열 삭제 → 레벨 수치가 2개 이상이고 MaxCount > 1이면 강화 가능
    public bool CanUpgradeRune => maxCount > 1 && levelValues != null && levelValues.Length > 1;
    public bool HasLevelValues => levelValues != null && levelValues.Length > 0;
    public float GetLevelValue(int lv) => levelValues[Mathf.Clamp(lv - 1, 0, levelValues.Length - 1)];
    // 최대 레벨을 넘는 레벨(드워프 룬 대장장이 보너스 등): 마지막 두 값의 차이만큼 계속 늘림
    public float GetLevelValueExtended(int lv)
    {
        int n = levelValues.Length;
        if (lv <= n || n < 2)
            return GetLevelValue(lv);
        float step = levelValues[n - 1] - levelValues[n - 2];
        return levelValues[n - 1] + step * (lv - n);
    }

    // public string valueInfo;
    [FormerlySerializedAs("itemPrefab")] public Rune runePrefab;
    public ConditionData[] unlockConditions; // 추가 효과 해금 조건 (모두 충족해야 효과 활성화)
    public int applyOrder; // 아이템 적용 순서
    public Sprite thum;

    // public Color color;

    public bool CheckUnlock()
    {
        bool unlocked = true;
        if (unlockConditions == null)
            return unlocked;

        for (int i = 0; i < unlockConditions.Length; i++)
        {
            if (!unlockConditions[i].Unlock())
            {
                unlocked = false;
            }
        }
        return unlocked;
    }

    public string GetDescription()
    {
        return runePrefab.GetDescription();
    }

    public static RuneData GetRuneData(string key)
    {
        return RuneManager.Instance.GetRuneData(key);
    }


    // "1.3/1.5/1.7" → [1.3, 1.5, 1.7] (빈 칸 = 레벨 수치 없음)
    public static float[] ParseLevelValues(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new float[0];
        string[] parts = raw.Split('/');
        // (2026-11-21) 시트에 "1/2/3"을 쓰면 엑셀이 날짜(2001/2/3)로 바꿔 버림 → 첫 칸 20xx는 xx로 되돌림 (시트 칸 서식을 '텍스트'로 두면 안 생김)
        if (parts.Length == 3 && parts[0].Trim().Length == 4 && parts[0].Trim().StartsWith("20")
            && int.TryParse(parts[0].Trim(), out int year))
        {
            Debug.LogWarning($"[RuneData] Values '{raw}'가 날짜로 바뀐 것으로 보여 '{year - 2000}/{parts[1]}/{parts[2]}'로 읽음 (시트 칸 서식을 텍스트로)");
            parts[0] = (year - 2000).ToString();
        }
        return System.Array.ConvertAll(parts, s => float.Parse(s.Trim(), System.Globalization.CultureInfo.InvariantCulture));
    }

#if UNITY_EDITOR
    public void LoadData()
    {
        TableReader table = TableReader.Load("RuneData.csv");
        if (table == null) return;

        TableReader.Row row = table.Find("Key", key);
        if (row == null)
        {
            Debug.LogWarning($"[RuneData] CSV에서 '{key}' 를 찾지 못함");
            return;
        }

        if (table.HasColumn("Name"))
            runeName = row.Get("Name");
        if (table.HasColumn("Desc"))
            desc = row.Get("Desc");
        if (row.TryGetInt("Level", out int lv))
            level = lv;
        else if (level <= 0)
            level = 1; // (2026-11-26) Level 열 삭제 → 그림·프리팹 폴더는 Level1
        // if (row.TryGetInt("AddPrice", out int ap))
        //     addPrice = ap;
        if (row.TryGetInt("MaxCount", out int mc))
            maxCount = mc;
        diceCoin = row.TryGetInt("DiceCoin", out int dc) ? dc : 0;
        levelValues = table.HasColumn("Values") ? ParseLevelValues(row.Get("Values")) : new float[0];

        string thumPath = $"Assets/2.Sprites/Rune/Level{level}/{key}.png";
        thum = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(thumPath);
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[RuneData] {key} LoadData 완료");
    }

    public void Edit()
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("RuneData Edit() key 널 입력하세요");
        }

        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(assetPath))
            name = key;
        else if (name != key)
        {
            string error = UnityEditor.AssetDatabase.RenameAsset(assetPath, key);
            if (!string.IsNullOrEmpty(error))
                Debug.LogError($"[RuneData] 이름 변경 실패: {error}");
        }
        string prefabRootFolder = $"Assets/3.Prefabs/Rune/Level{level}";

        // 기존 프리팹 탐색 (grade 폴더 → 루트 폴더 순)
        string[] searchFolders = new[] { prefabRootFolder };
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"{key}Rune t:Prefab", searchFolders);
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) != key) continue;

            Rune prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<Rune>(path);
            if (prefab != null)
            {
                runePrefab = prefab;
                runePrefab.key = key;
                UnityEditor.EditorUtility.SetDirty(runePrefab); // 프리팹 key도 저장되도록
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"[RuneData] {key}Rune prefab 연결 완료: {path}");
                return;
            }
        }

        // 프리팹 없으면 grade 폴더에 새로 생성
        if (!UnityEditor.AssetDatabase.IsValidFolder(prefabRootFolder))
            System.IO.Directory.CreateDirectory(prefabRootFolder);

        var go = new GameObject(key);

        // key+"Rune" 이름의 타입이 존재하면 컴포넌트 추가, 없으면 스크립트 파일 생성
        string componentTypeName = key + "Rune";
        System.Type runeType = System.Type.GetType(componentTypeName);
        if (runeType != null && typeof(Rune).IsAssignableFrom(runeType))
        {
            go.AddComponent(runeType);
            Debug.Log($"[RuneData] {componentTypeName} 컴포넌트 추가됨");
        }
        else
        {
            string scriptPath = CreateRuneScript(componentTypeName);
            Debug.LogWarning($"[RuneData] {componentTypeName} 타입 없음 → 스크립트 생성: {scriptPath}\n컴파일 완료 후 Edit을 다시 눌러주세요.");
            DestroyImmediate(go);
            UnityEditor.AssetDatabase.Refresh();
            return;
        }

        string newPath = $"{prefabRootFolder}/{key}.prefab";
        var newPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, newPath);
        DestroyImmediate(go);

        runePrefab = newPrefab.GetComponent<Rune>();
        runePrefab.key = key;
        UnityEditor.EditorUtility.SetDirty(runePrefab);
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[RuneData] {key} 새 prefab 생성 완료: {prefabRootFolder}");
    }

    string CreateRuneScript(string className)
    {
        string scriptFolder = $"Assets/1.Scripts/InGame/Player/Item/Level{level}";
        string filePath = $"{scriptFolder}/{className}.cs";

        if (System.IO.File.Exists(filePath))
            return filePath;

        string content =
$@"using UnityEngine;

public class {className} : Rune
{{
    public override void OnEquip(Player player)
    {{
        base.OnEquip(player);
    }}

    public override void UpdateRune()
    {{
    }}

    public override void OnUnequip(Player player)
    {{
    }}
}}
";
        System.IO.File.WriteAllText(filePath, content);
        return filePath;
    }
#endif
}

public enum RuneApplyType
{
    Tile,
    Character,
    None
}
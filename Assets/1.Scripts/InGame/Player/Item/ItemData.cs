using UnityEngine;

[System.Flags]
public enum AcquireMethod
{
    Purchase = 1 << 0,
    Select = 1 << 1,
    Merge = 1 << 2,
}

[CreateAssetMenu]
public class ItemData : ScriptableObject
{
    public string key;
    public string Title => itemName;
    public string itemName;
    public string desc;
    public int level;//무기의 레벨 1부터 ->10까지
    public int maxOwnCount; //최대 소유 개수 
    public bool hideDisplay; // true면 보유 현황 UI에 표시 안 함 - 물약용
    public AcquireMethod acquireMethod;
    public Sprite thumbnail;
    //[SerializeField] int price;

    public int GetPrice()
    {
        return prices[level - 1];
    }
    public Grade grade;      // 등급별 필터링용
    public Item itemPrefab;
    public ConditionData[] addEffectUnlockConditions; // 추가 효과 해금 조건 (모두 충족해야 효과 활성화)
    public int applyOrder; // 아이템 적용 순서
    public string[] mergeItemKeys; // 재료가 될 수 있는 상위 아이템
    public string[] IncludingItems; // 보유 시 보유처리되는 아이템 리스트
    // 실제 효과는 Item 컴포넌트(or Strategy)로 분리

    public static Color GetGradeColor(Grade grade)
    {
        return grade switch
        {
            Grade.Normal => new Color(0.78f, 0.78f, 0.78f), // 회색
            Grade.Rare => new Color(0.00f, 0.44f, 0.87f), // 파랑
            Grade.Unique => new Color(0.56f, 0.28f, 0.98f), // 보라
            Grade.Legend => new Color(1.00f, 0.64f, 0.00f), // 주황/금
            Grade.Myth => new Color(0.98f, 0.18f, 0.18f), // 빨강
            _ => Color.white
        };
    }

    public string GetDescription(int count = 1)
    {
        return itemPrefab.GetDescription(count);
    }

    public static ItemData GetItemData(string key)
    {
        return ItemManager.Instance.GetItemData(key);
    }

#if UNITY_EDITOR
    public void LoadData()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "Json/ItemData.csv");
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"[ItemData] CSV 파일 없음: {path}");
            return;
        }

        string[] lines = System.IO.File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = ParseCsvLine(lines[0]);
        for (int i = 0; i < headers.Length; i++)
            headers[i] = headers[i].Trim();

        int iKey = System.Array.IndexOf(headers, "Key");
        int iName = System.Array.IndexOf(headers, "Name");
        int iDesc = System.Array.IndexOf(headers, "Desc");
        int iAcquire = System.Array.IndexOf(headers, "Purchase,Select");
        int iGrade = System.Array.IndexOf(headers, "Grade");
        int iLevel = System.Array.IndexOf(headers, "Level");
        int iHide = System.Array.IndexOf(headers, "hideDisplay");
        int iOwn = System.Array.IndexOf(headers, "OwnCount");
        int iIncluding = System.Array.IndexOf(headers, "IncludingItems");
        int iMerge = System.Array.IndexOf(headers, "MergeItems");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = ParseCsvLine(lines[i]);

            string rowKey = iKey >= 0 && iKey < cols.Length ? cols[iKey].Trim() : "";
            if (rowKey != key) continue;

            if (iName >= 0 && iName < cols.Length)
                itemName = cols[iName].Trim();
            if (iDesc >= 0 && iDesc < cols.Length)
                desc = cols[iDesc].Trim();
            if (iAcquire >= 0 && iAcquire < cols.Length)
            {
                acquireMethod = 0;
                string[] methods = cols[iAcquire].Split(',');
                foreach (var m in methods)
                {
                    if (System.Enum.TryParse<AcquireMethod>(m.Trim(), out var am))
                        acquireMethod |= am;
                }
            }
            if (iGrade >= 0 && iGrade < cols.Length && System.Enum.TryParse<Grade>(cols[iGrade].Trim(), out var g))
                grade = g;
            if (iLevel >= 0 && iLevel < cols.Length && int.TryParse(cols[iLevel].Trim(), out int lv))
                level = lv;
            if (iHide >= 0 && iHide < cols.Length)
                hideDisplay = cols[iHide].Trim().ToUpper() == "TRUE";
            if (iOwn >= 0 && iOwn < cols.Length && int.TryParse(cols[iOwn].Trim(), out int own))
                maxOwnCount = own;
            if (iIncluding >= 0 && iIncluding < cols.Length)
            {
                string raw = cols[iIncluding].Trim();
                IncludingItems = string.IsNullOrEmpty(raw) ? new string[0] : raw.Split(';');
            }
            if (iMerge >= 0 && iMerge < cols.Length)
            {
                string raw = cols[iMerge].Trim();
                mergeItemKeys = string.IsNullOrEmpty(raw) ? new string[0] : raw.Split(';');
            }

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[ItemData] {key} LoadData 완료");
            return;
        }

        Debug.LogWarning($"[ItemData] CSV에서 '{key}' 를 찾지 못함");
    }

    static string[] ParseCsvLine(string line)
    {
        var result = new System.Collections.Generic.List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"') { inQuotes = !inQuotes; continue; }
            if (c == ',' && !inQuotes) { result.Add(current.ToString()); current.Clear(); continue; }
            current.Append(c);
        }
        result.Add(current.ToString());
        return result.ToArray();
    }

    public void Edit()
    {
        if (string.IsNullOrEmpty(key) || key != name)
            key = name;

        string prefabRootFolder = "Assets/3.Prefabs/Item";
        string gradeFolder = $"{prefabRootFolder}/{grade}";

        // 기존 프리팹 탐색 (grade 폴더 → 루트 폴더 순)
        string[] searchFolders = UnityEditor.AssetDatabase.IsValidFolder(gradeFolder)
            ? new[] { gradeFolder, prefabRootFolder }
            : new[] { prefabRootFolder };
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"{key}Item t:Prefab", searchFolders);
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) != key) continue;

            Item prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<Item>(path);
            if (prefab != null)
            {
                itemPrefab = prefab;
                itemPrefab.key = key;
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"[ItemData] {key}Item prefab 연결 완료: {path}");
                return;
            }
        }

        // 프리팹 없으면 grade 폴더에 새로 생성
        if (!UnityEditor.AssetDatabase.IsValidFolder(prefabRootFolder))
            System.IO.Directory.CreateDirectory(prefabRootFolder);
        if (!UnityEditor.AssetDatabase.IsValidFolder(gradeFolder))
            UnityEditor.AssetDatabase.CreateFolder(prefabRootFolder, grade.ToString());

        var go = new GameObject(key);

        // key+"Item" 이름의 타입이 존재하면 컴포넌트 추가, 없으면 스크립트 파일 생성
        string componentTypeName = key + "Item";
        System.Type itemType = System.Type.GetType(componentTypeName);
        if (itemType != null && typeof(Item).IsAssignableFrom(itemType))
        {
            go.AddComponent(itemType);
            Debug.Log($"[ItemData] {componentTypeName} 컴포넌트 추가됨");
        }
        else
        {
            string scriptPath = CreateItemScript(componentTypeName);
            Debug.LogWarning($"[ItemData] {componentTypeName} 타입 없음 → 스크립트 생성: {scriptPath}\n컴파일 완료 후 Edit을 다시 눌러주세요.");
            DestroyImmediate(go);
            UnityEditor.AssetDatabase.Refresh();
            return;
        }

        string newPath = $"{gradeFolder}/{key}Item.prefab";
        var newPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, newPath);
        DestroyImmediate(go);

        itemPrefab = newPrefab.GetComponent<Item>();
        itemPrefab.key = key;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[ItemData] {key} 새 prefab 생성 완료: {newPath}");
    }

    string CreateItemScript(string className)
    {
        string scriptFolder = "Assets/1.Scripts/InGame/Player/Item/";
        string filePath = $"{scriptFolder}/{className}.cs";

        if (System.IO.File.Exists(filePath))
            return filePath;

        string content =
$@"using UnityEngine;

public class {className} : Item
{{
    public override void OnEquip(Player player)
    {{
        base.OnEquip(player);
    }}

    public override void UpdateItem()
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
    int[] prices = { 10, 25, 45, 70, 100, 135, 175, 220, 270, 325 };

}
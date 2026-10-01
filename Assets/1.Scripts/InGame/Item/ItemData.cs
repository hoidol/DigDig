using UnityEngine;

[System.Flags]
public enum AcquireMethod
{
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
    // public int consumeHp;
    public int level;
    public int addPrice; //추가 금액
    public int maxCount = 2; //최대 보유 개수

    public string valueInfo;
    public Item itemPrefab;
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
        return itemPrefab.GetDescription();
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

        int iKey = System.Array.IndexOf(headers, "key");
        int iName = System.Array.IndexOf(headers, "name");
        int iDesc = System.Array.IndexOf(headers, "desc");
        int iLevel = System.Array.IndexOf(headers, "level");
        int iAddPrice = System.Array.IndexOf(headers, "addPrice");
        int iMaxCount = System.Array.IndexOf(headers, "maxCount");

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
            if (iLevel >= 0 && iLevel < cols.Length && int.TryParse(cols[iLevel].Trim(), out int lv))
                level = lv;
            if (iAddPrice >= 0 && iAddPrice < cols.Length && int.TryParse(cols[iAddPrice].Trim(), out int ap))
                addPrice = ap;
            if (iMaxCount >= 0 && iMaxCount < cols.Length && int.TryParse(cols[iMaxCount].Trim(), out int mc))
                maxCount = mc;

            string thumPath = $"Assets/2.Sprites/Item/Level{level}/{key}.png";
            thum = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(thumPath);
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[ItemData] {key} LoadData 완료");
            return;
        }

        //


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

        string prefabRootFolder = $"Assets/3.Prefabs/Item/Level{level}";

        // 기존 프리팹 탐색 (grade 폴더 → 루트 폴더 순)
        string[] searchFolders = new[] { prefabRootFolder };
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

        string newPath = $"{prefabRootFolder}/{key}.prefab";
        var newPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, newPath);
        DestroyImmediate(go);

        itemPrefab = newPrefab.GetComponent<Item>();
        itemPrefab.key = key;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[ItemData] {key} 새 prefab 생성 완료: {prefabRootFolder}");
    }

    string CreateItemScript(string className)
    {
        string scriptFolder = $"Assets/1.Scripts/InGame/Player/Item/Level{level}";
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
}
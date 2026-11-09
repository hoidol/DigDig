using UnityEngine;



[CreateAssetMenu]
public class ItemData : ScriptableObject
{
    public string key;
    public string Title => itemName;
    public string itemName;
    public string desc;
    // public int consumeHp;
    public int level;
    // public int addPrice; //추가 금액
    public int maxCount = 2; //최대 보유 개수, 타일 아이템은 최대 강화 레벨도 겸함

    // 타일 아이템 강화 (CLAUDE.md "타일 아이템 강화"): 비용 = diceCoin × (현재 레벨 + 1), 0이면 강화 불가
    public int diceCoin;
    public float[] levelValues; // ItemData.csv Values ("1.3/1.5/1.7"), index = 레벨 - 1
    public int MaxLevel => maxCount;
    public bool CanUpgradeItem => diceCoin > 0 && levelValues != null && levelValues.Length > 1;
    public bool HasLevelValues => levelValues != null && levelValues.Length > 0;
    public float GetLevelValue(int lv) => levelValues[Mathf.Clamp(lv - 1, 0, levelValues.Length - 1)];

    // public string valueInfo;
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


    // "1.3/1.5/1.7" → [1.3, 1.5, 1.7] (빈 칸 = 레벨 수치 없음)
    public static float[] ParseLevelValues(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new float[0];
        return System.Array.ConvertAll(raw.Split('/'), s => float.Parse(s.Trim(), System.Globalization.CultureInfo.InvariantCulture));
    }

#if UNITY_EDITOR
    public void LoadData()
    {
        TableReader table = TableReader.Load("ItemData.csv");
        if (table == null) return;

        TableReader.Row row = table.Find("Key", key);
        if (row == null)
        {
            Debug.LogWarning($"[ItemData] CSV에서 '{key}' 를 찾지 못함");
            return;
        }

        if (table.HasColumn("Name"))
            itemName = row.Get("Name");
        if (table.HasColumn("Desc"))
            desc = row.Get("Desc");
        if (row.TryGetInt("Level", out int lv))
            level = lv;
        // if (row.TryGetInt("AddPrice", out int ap))
        //     addPrice = ap;
        if (row.TryGetInt("MaxCount", out int mc))
            maxCount = mc;
        diceCoin = row.TryGetInt("DiceCoin", out int dc) ? dc : 0;
        levelValues = table.HasColumn("Values") ? ParseLevelValues(row.Get("Values")) : new float[0];

        string thumPath = $"Assets/2.Sprites/Item/Level{level}/{key}.png";
        thum = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(thumPath);
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[ItemData] {key} LoadData 완료");
    }

    public void Edit()
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("ItemData Edit() key 널 입력하세요");
        }

        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(assetPath))
            name = key;
        else if (name != key)
        {
            string error = UnityEditor.AssetDatabase.RenameAsset(assetPath, key);
            if (!string.IsNullOrEmpty(error))
                Debug.LogError($"[ItemData] 이름 변경 실패: {error}");
        }
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
                UnityEditor.EditorUtility.SetDirty(itemPrefab); // 프리팹 key도 저장되도록
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
        UnityEditor.EditorUtility.SetDirty(itemPrefab);
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

public enum ItemApplyType
{
    Tile,
    Character,
    None
}
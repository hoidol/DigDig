using UnityEngine;

[CreateAssetMenu]
public class ItemData : ScriptableObject
{
    public string key;
    public string title;
    public int maxOwnCount;
    public const int MAX_OWN_COUNT = 3;
    public bool isUnique; //유일한 아이템
    public string GetDescription(int count = 1)
    {
        return itemPrefab.GetDescription(count);
    }
    public Sprite thumbnail;
    [SerializeField] int price;
    public int GetPrice()
    {
        return price;
    }
    public Grade grade;      // 등급별 필터링용
    public Item itemPrefab;
    public int applyOrder; // 아이템 적용 순서
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

    public static ItemData GetItemData(string key)
    {
        return ItemManager.Instance.GetItemData(key);
    }

#if UNITY_EDITOR
    public void Edit()
    {
        if (string.IsNullOrEmpty(key) || key != name)
            key = name;

        string prefabFolder = "Assets/3.Prefabs/Item";

        // 기존 프리팹 탐색
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"{key} t:Prefab", new[] { prefabFolder });
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
                Debug.Log($"[ItemData] {key} prefab 연결 완료: {path}");
                return;
            }
        }

        // 프리팹 없으면 새로 생성
        if (!UnityEditor.AssetDatabase.IsValidFolder(prefabFolder))
            System.IO.Directory.CreateDirectory(prefabFolder);

        var go = new GameObject(key);

        // key+"Item" 이름의 타입이 존재하면 컴포넌트 추가
        string componentTypeName = key + "Item";
        System.Type itemType = System.Type.GetType(componentTypeName);
        if (itemType != null && typeof(Item).IsAssignableFrom(itemType))
        {
            go.AddComponent(itemType);
            Debug.Log($"[ItemData] {componentTypeName} 컴포넌트 추가됨");
        }
        else
        {
            Debug.LogWarning($"[ItemData] {componentTypeName} 타입을 찾지 못해 빈 GameObject로 생성됨");
        }

        string newPath = $"{prefabFolder}/{key}.prefab";
        var newPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, newPath);
        DestroyImmediate(go);

        itemPrefab = newPrefab.GetComponent<Item>();
        itemPrefab.key = key;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[ItemData] {key} 새 prefab 생성 완료: {newPath}");
    }
#endif

}
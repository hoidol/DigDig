using UnityEngine;

[CreateAssetMenu(fileName = "AbilityData", menuName = "Ability/AbilityData", order = 0)]
public class AbilityData : ScriptableObject
{
    public string key;
    public Sprite thum;
    public Grade grade;
    [Header("-1이면 레벨업 제한없음, 또는 5, Ability lv 5부터 아이템이 진화함")]
    public int maxLv = -1;
    public Ability abilityPrefab;
    public ConditionData[] conditions;
    public string[] synergyItems;
    [Header("높을 수록 후순위 - 능력들이 동시 적용될때의 순서 설정")]
    public int applyOrder;

    public bool Unlocked()
    {
        foreach (var condition in conditions)
        {
            if (!condition.Check())
                return false;
        }
        return true;
    }
    public string Title => key;
    public string Description(int c = -1)
    {
        if (c < 0)
            c = 1;
        return abilityPrefab.GetDescription(c);
    }

    public static AbilityData GetAbilityData(string key)
    {
        return AbilityManager.Instance.GetAbilityData(key);
    }

#if UNITY_EDITOR
    public void Edit()
    {
        if (string.IsNullOrEmpty(key) || key != name)
            key = name;

        string prefabFolder = "Assets/3.Prefabs/Ability";

        // 기존 프리팹 탐색
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"{key} t:Prefab", new[] { prefabFolder });
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) != key) continue;

            Ability prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<Ability>(path);
            if (prefab != null)
            {
                abilityPrefab = prefab;
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"[AbilityData] {key} prefab 연결 완료: {path}");
                return;
            }
        }

        // 프리팹 없으면 새로 생성
        if (!UnityEditor.AssetDatabase.IsValidFolder(prefabFolder))
            System.IO.Directory.CreateDirectory(prefabFolder);

        var go = new GameObject(key);

        // key+"Ability" 이름의 타입이 존재하면 컴포넌트 추가
        string componentTypeName = key + "Ability";
        System.Type abilityType = System.Type.GetType(componentTypeName);
        if (abilityType != null && typeof(Ability).IsAssignableFrom(abilityType))
        {
            go.AddComponent(abilityType);
            Debug.Log($"[AbilityData] {componentTypeName} 컴포넌트 추가됨");
        }
        else
        {
            Debug.LogWarning($"[AbilityData] {componentTypeName} 타입을 찾지 못해 빈 GameObject로 생성됨");
        }

        string newPath = $"{prefabFolder}/{key}.prefab";
        var newPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, newPath);
        DestroyImmediate(go);

        abilityPrefab = newPrefab.GetComponent<Ability>();
        abilityPrefab.key = key;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[AbilityData] {key} 새 prefab 생성 완료: {newPath}");
    }
#endif


}


using UnityEngine;

[CreateAssetMenu(fileName = "AbilityData", menuName = "Ability/AbilityData", order = 0)]
public class AbilityData : ScriptableObject
{
    public string key;
    public int level;

    public string desc;

    public Sprite thum;
    //public Grade grade;
    [Header("-1이면 레벨업 제한없음, 또는 5, Ability lv 5부터 아이템이 진화함")]
    public int maxLv = -1;
    public Ability abilityPrefab;
    public ConditionData[] conditions;
    public AbilityType abilityType;
    [Header("높을 수록 후순위 - 능력들이 동시 적용될때의 순서 설정")]
    public int applyOrder;

    public virtual bool Unlocked()
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
    public virtual void LoadData()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "Json/AbilityData.csv");
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"[AbilityData] CSV 파일 없음: {path}");
            return;
        }

        string[] lines = System.IO.File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2)
        {
            Debug.Log("if (lines.Length < 2)");
            return;
        }

        string[] headers = lines[0].Split(',');
        int iKey = System.Array.IndexOf(headers, "Key");
        int iDesc = System.Array.IndexOf(headers, "Desc");
        int iLevel = System.Array.IndexOf(headers, "Level");
        int iMaxLv = System.Array.IndexOf(headers, "MaxLv");
        int iTotalCount = System.Array.IndexOf(headers, "TotalAbilityCount");
        int iAbilityType = System.Array.IndexOf(headers, "AbilityType");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split(',');

            string rowKey = iKey >= 0 && iKey < cols.Length ? cols[iKey].Trim() : "";
            if (rowKey != key) continue;

            if (iDesc >= 0 && iDesc < cols.Length)
                desc = cols[iDesc].Trim();

            if (iLevel >= 0 && iLevel < cols.Length && int.TryParse(cols[iLevel].Trim(), out int lv))
                level = lv;

            if (iMaxLv >= 0 && iMaxLv < cols.Length && int.TryParse(cols[iMaxLv].Trim(), out int mlv))
                maxLv = mlv;

            if (iAbilityType >= 0 && iAbilityType < cols.Length &&
                System.Enum.TryParse<AbilityType>(cols[iAbilityType].Trim(), out var parsedType))
                abilityType = parsedType;

            var condList = new System.Collections.Generic.List<ConditionData>();
            if (iTotalCount >= 0 && iTotalCount < cols.Length &&
                int.TryParse(cols[iTotalCount].Trim(), out int totalCount) && totalCount > 0)
            {
                condList.Add(new ConditionData
                {
                    conditionType = ConditionType.TotalAbilityCount,
                    count = totalCount
                });
            }
            conditions = condList.ToArray();

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[AbilityData] {key} LoadData 완료 (level={level}, maxLv={maxLv}, type={abilityType})");
            return;
        }

        Debug.LogWarning($"[AbilityData] CSV에서 key '{key}' 를 찾지 못함");
    }

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

public enum AbilityType
{
    Base,
    Core,
    Synergy
}
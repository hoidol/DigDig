using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "SlimeData", menuName = "SlimeData/SlimeData")]
public class SlimeData : ScriptableObject
{
    public const int MAX_MERGE_LEVEL = 4; // mergeLevel 0 ~ MAX_MERGE_LEVEL
    public static readonly int MAX_ENHANCE_LEVEL = 15;
    public static int[] ABILITY_LEVELS = { 2, 5, 8, 11, 14 };

    public string key;
    public string Title => TranslateManager.GetText(key);
    public string desc;
    // public string[] enhanceAbilityDescs;
    public int growth;
    public AttackType attackType;
    public SlimeEnhanceInfo[] commonSlimeEnhanceInfos;

    [Tooltip("공격력 랜덤 폭. 0이면 고정, 0.67이면 중간값 ±67% (9 → 3~15). 데이터에는 중간값만 입력")]
    [Range(0f, 1f)] public float attackPowerRandomRatio;

    public float GetRandomRatio(StatType statType)
    {
        return statType == StatType.AttackPower ? attackPowerRandomRatio : 0f;
    }

    // 중간값 기준 ±ratio 범위에서 랜덤
    public static float ApplyRandomRatio(float mid, float ratio)
    {
        if (ratio <= 0f)
            return mid;
        return Random.Range(mid * (1f - ratio), mid * (1f + ratio));
    }


    [Tooltip("인게임 직접 강화 1레벨당 공격력 증가(%). 종마다 다름 (SlimeData.csv ReinforceAttackPower)")]
    public float reinforceAttackPowerPercent = 10f;

    public Slime prefab;
    public ConditionData[] unlockConditions; // 추가 효과 해금 조건 (모두 충족해야 효과 활성화)

    public Sprite thum;
    public GradeType grade;
    public SlimeEnhanceInfo GetCommonSlimeEnhanceInfo(int lv)
    {
        return commonSlimeEnhanceInfos.Where(e => e.level == lv).FirstOrDefault();
    }


    // UI에 노출할 스탯 종류와 순서. 에셋별 인스펙터에서 설정한다. (예: Flame은 FlameDuration은 빼고 FlameDPS까지만)
    public List<StatType> displayStatTypes;

    public virtual List<SlimeStat> GetDisplaySlimeStats(int enhanceLv)
    {
        var result = new List<SlimeStat>();
        foreach (StatType statType in displayStatTypes)
        {
            SlimeStat stat = GetSlimeStat(statType, enhanceLv);
            if (stat != null)
                result.Add(stat);
        }
        return result;
    }

    // 강화 레벨과 상관없는 기본 스탯 (SlimeData.csv의 AttackSpeed, AttackRange, CritChance 열. 값 하나 = 머지 레벨 무관 고정)
    public List<SlimeStat> baseSlimeStats = new List<SlimeStat>();
    public SlimeStat GetBaseStat(StatType statType) => baseSlimeStats?.FirstOrDefault(e => e.statType == statType);

    // 기본 스탯(SlimeData.csv) → 강화 레벨별 스탯(SlimeEnhance csv) 순서로 찾음
    public virtual SlimeStat GetSlimeStat(StatType statType, int enhanceLv)
    {
        SlimeStat baseStat = GetBaseStat(statType);
        if (baseStat != null)
            return baseStat;
        SlimeEnhanceInfo slimeEnhanceInfo = GetCommonSlimeEnhanceInfo(enhanceLv);
        return slimeEnhanceInfo?.GetSlimeStat(statType);
    }

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

    public virtual string GetDescription(int mergeLv = 0)
    {
        return desc;
    }

    public static SlimeData GetSlimeData(string key)
    {
        return SlimeManager.Instance.GetSlimeData(key);
    }




#if UNITY_EDITOR
    public virtual void LoadData()
    {
        // 새로 만든 에셋은 key가 비어 있음 → 파일명(= Key)으로 채운다 (Edit과 같은 규칙)
        if (string.IsNullOrEmpty(key))
            key = name;

        TableReader table = TableReader.Load("SlimeData.csv");
        if (table == null) return;

        TableReader.Row row = table.Find("Key", key);
        if (row == null)
        {
            Debug.LogWarning($"[SlimeData] CSV에서 '{key}' 를 찾지 못함");
            return;
        }
        displayStatTypes = new List<StatType> { StatType.AttackPower, StatType.AttackSpeed, StatType.AttackRange };
        if (table.HasColumn("Desc"))
            desc = row.Get("Desc");
        if (row.TryGetInt("Growth", out int lv))
            growth = lv;
        if (row.TryGetEnum("Grade", out GradeType parsedGrade))
            grade = parsedGrade;
        if (table.HasColumn("AttackType"))
        {
            if (row.TryGetEnum("AttackType", out AttackType parsedAttackType))
                attackType = parsedAttackType;
            else
                Debug.LogWarning($"[SlimeData] {key} AttackType '{row.Get("AttackType")}' 파싱 실패");
        }
        if (row.TryGetFloat("ReinforceAttackPower", out float reinforcePercent))
            reinforceAttackPowerPercent = reinforcePercent;

        // 강화 레벨과 상관없는 기본 스탯 (2026-11-06 SlimeEnhance에서 이동). 값 하나 = 머지 레벨과 상관없이 고정
        baseSlimeStats = new List<SlimeStat>();
        foreach (StatType statType in new[] { StatType.AttackSpeed, StatType.AttackRange, StatType.CritChance })
        {
            string raw = table.HasColumn(statType.ToString()) ? row.Get(statType.ToString()) : null;
            if (string.IsNullOrWhiteSpace(raw))
                continue;
            baseSlimeStats.Add(new SlimeStat
            {
                statType = statType,
                values = raw.Split('/').Select(v => v.Trim()).ToArray(),
            });
        }

        prefab = FindPrefab();
        thum = FindAssetByName<Sprite>($"Assets/2.Sprites/Slime/Growth{growth}", key);

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[SlimeData] {key} LoadData 완료");
    }

    // 슬라임 프리팹 폴더: 등급별 (3.Prefabs/Slime/{C,B,A,S}/{key}Slime.prefab). grade는 CSV Grade 컬럼
    const string PREFAB_ROOT = "Assets/3.Prefabs/Slime";
    string PrefabFolder => $"{PREFAB_ROOT}/{grade}";

    // 등급 폴더에서 찾고, 없으면 슬라임 프리팹 전체에서 찾아 등급 폴더와 다르다고 경고 (등급 변경 후 프리팹을 안 옮긴 경우)
    Slime FindPrefab()
    {
        string fileName = $"{key}Slime";
        string expectedPath = $"{PrefabFolder}/{fileName}.prefab";
        Slime found = UnityEditor.AssetDatabase.LoadAssetAtPath<Slime>(expectedPath);
        if (found != null)
            return found;

        foreach (string guid in UnityEditor.AssetDatabase.FindAssets($"{fileName} t:Prefab", new[] { PREFAB_ROOT }))
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) != fileName)
                continue;

            found = UnityEditor.AssetDatabase.LoadAssetAtPath<Slime>(path);
            if (found != null)
            {
                Debug.LogWarning($"[SlimeData] {key} 프리팹이 등급 폴더에 없음 (등급 {grade}) → {path} 사용. {PrefabFolder}로 옮겨 주세요.");
                return found;
            }
        }

        Debug.LogWarning($"[SlimeData] {key} 프리팹 없음 : {expectedPath}");
        return null;
    }

    // folder 안에서 파일명이 fileName과 일치하는 T 타입 에셋을 찾는다.
    T FindAssetByName<T>(string folder, string fileName) where T : UnityEngine.Object
    {
        if (!UnityEditor.AssetDatabase.IsValidFolder(folder))
        {
            Debug.LogWarning($"[SlimeData] 폴더 없음: {folder}");
            return null;
        }

        string typeFilter = typeof(T) == typeof(Slime) ? "t:Prefab" : "t:Sprite";
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"{fileName} {typeFilter}", new[] { folder });
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) != fileName) continue;

            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
        }

        Debug.LogWarning($"[SlimeData] {folder} 에서 '{fileName}' 를 찾지 못함");
        return null;
    }


    public void Edit()
    {
        if (string.IsNullOrEmpty(key) || key != name)
            key = name;

        string prefabRootFolder = PrefabFolder;
        string prefabPath = $"{prefabRootFolder}/{key}Slime.prefab";

        // 이미 해당 경로에 프리팹이 있으면 그대로 연결만 하고 덮어쓰지 않는다.
        Slime existingPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<Slime>(prefabPath);
        if (existingPrefab != null)
        {
            prefab = existingPrefab;
            prefab.key = key;
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[SlimeData] {key} 기존 prefab 연결 완료: {prefabPath}");
            return;
        }

        // 프리팹 없으면 grade 폴더에 새로 생성
        if (!UnityEditor.AssetDatabase.IsValidFolder(prefabRootFolder))
            System.IO.Directory.CreateDirectory(prefabRootFolder);

        var go = new GameObject(key + "Slime");

        // key+"Slime" 이름의 타입이 존재하면 컴포넌트 추가, 없으면 스크립트 파일 생성
        string componentTypeName = key + "Slime";
        System.Type slimeType = System.Type.GetType(componentTypeName);
        if (slimeType != null && typeof(Slime).IsAssignableFrom(slimeType))
        {
            go.AddComponent(slimeType);
            Debug.Log($"[SlimeData] {componentTypeName} 컴포넌트 추가됨");
        }
        else
        {
            string scriptPath = CreateSlimeScript(componentTypeName);
            Debug.LogWarning($"[SlimeData] {componentTypeName} 타입 없음 → 스크립트 생성: {scriptPath}\n컴파일 완료 후 Edit을 다시 눌러주세요.");
            DestroyImmediate(go);
            UnityEditor.AssetDatabase.Refresh();
            return;
        }

        var newPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        DestroyImmediate(go);

        prefab = newPrefab.GetComponent<Slime>();
        prefab.key = key;
        newPrefab.AddComponent<SlimeMovement>();
        Rigidbody2D rg2d = newPrefab.AddComponent<Rigidbody2D>();
        rg2d.freezeRotation = true;
        rg2d.gravityScale = 0;
        CircleCollider2D col = newPrefab.AddComponent<CircleCollider2D>();
        col.offset = new Vector2(0, 0.075f);
        col.radius = 0.4f;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[SlimeData] {key} 새 prefab 생성 완료: {prefabRootFolder}");
    }

    string CreateSlimeScript(string className)
    {
        string scriptFolder = $"Assets/1.Scripts/InGame/Slime/Unit/{grade}/{className}";
        System.IO.Directory.CreateDirectory(scriptFolder);
        string filePath = $"{scriptFolder}/{className}.cs";

        if (System.IO.File.Exists(filePath))
            return filePath;

        string content =
$@"using UnityEngine;

public class {className} : Slime
{{

}}
";
        System.IO.File.WriteAllText(filePath, content);
        return filePath;
    }
#endif
}

[System.Serializable]
public class SlimeEnhanceInfo
{
    public int level;
    public SlimeStat[] slimeStats;
    public SlimeStat GetSlimeStat(StatType slimeStatType)
    {
        return slimeStats.Where(e => e.statType == slimeStatType).FirstOrDefault();
    }
}

[System.Serializable]
public class SlimeStat
{
    public StatType statType;
    public string[] values;
    public string Title => statType.ToString();
    public Sprite Thum
    {
        get
        {
            var sprite = Resources.Load<Sprite>($"Icon/Stat/{statType}");
            return sprite;
        }
    }

    // values 형식
    // - 2개: "기본/증가량" → 기본 + 증가량 * mergeLv
    // - 1개 또는 3개 이상: mergeLv별 값 (모자라면 마지막 값 사용)
    bool IsLinear => values.Length == 2;

    // randomRatio > 0이면 중간값 기준 "최소~최대"로 표시
    public string GetValueToString(int mergeLv, float randomRatio = 0f)
    {
        if (randomRatio > 0f)
            return FormatValue(GetValue<float>(mergeLv), randomRatio);

        if (IsLinear && !values[0].Contains("~"))
            return FormatValue(GetValue(values[0]) + GetValue(values[1]) * mergeLv);

        return values.GetByMergeLevel(mergeLv);
    }

    // 이미 계산된 값(공격력 등 스탯 값)을 같은 표시 형식으로 변환
    public static string FormatValue(float value, float randomRatio = 0f)
    {
        if (randomRatio > 0f)
            return $"{value * (1f - randomRatio):0.#}~{value * (1f + randomRatio):0.#}";

        return value.ToString("0.##");
    }

    public T GetValue<T>(int mergeLv)
    {
        float result;
        if (IsLinear)
        {
            result = GetValue(values[0]) + GetValue(values[1]) * mergeLv;
        }
        else
        {
            result = GetValue(values.GetByMergeLevel(mergeLv));
        }

        if (typeof(T) == typeof(int))
            return (T)(object)Mathf.RoundToInt(result);

        return (T)(object)result;
    }

    float GetValue(string valueString)
    {
        if (string.IsNullOrEmpty(valueString))
            return 0;

        if (valueString.Contains("~"))
        {
            string[] parts = valueString.Split('~');
            if (parts.Length == 2)
            {
                float minValue = float.Parse(parts[0]);
                float maxValue = float.Parse(parts[1]);
                return Random.Range(minValue, maxValue);
            }
        }
        else
        {
            return float.Parse(valueString);
        }

        return 0;
    }
}


// public enum StatType
// {
//     AttackPower, //float
//     AttackSpeed, //float 10초동안 몇발 쏘는지
//     AttackRange,
//     FlameDPS,
//     Count
// }
public static class MergeLevelExtensions
{
    // mergeLevel별 데이터 배열 조회. 배열이 짧으면 마지막 값을 사용 (MAX_MERGE_LEVEL 늘려도 IndexOutOfRange 방지)
    public static T GetByMergeLevel<T>(this T[] array, int mergeLevel)
    {
        if (array == null || array.Length == 0)
            return default;
        return array[Mathf.Clamp(mergeLevel, 0, array.Length - 1)];
    }
}


public enum AttackType
{
    AllRounder, // 만능 모든 공격 타입 100
    Physic, //빨강
    Magic, //파랑
}
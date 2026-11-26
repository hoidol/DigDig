using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "MinionData", menuName = "MinionData/MinionData")]
public class MinionData : ScriptableObject
{
    public const int MAX_MERGE_LEVEL = 4; // mergeLevel 0 ~ MAX_MERGE_LEVEL (표시 5성 = mergeLevel 4가 최대. 2026-11-19 잠시 3(4성)으로 줄였다가 되돌림)

    public string key;
    public string Title => TranslateManager.GetText($"Minion.{key}"); // 이름은 Translate.csv (Minion.{Key})
    public string desc;
    public int growth;
    public AttackType attackType;
    [Tooltip("슬라임 유형 (MinionData.csv Tags). 형태 하나 + 특성. 장비가 장착 슬라임 구성에 반응하는 기준")]
    public MinionTag tags;

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


    [Tooltip("쿨타임 스킬이 있는 미니언 (머리 위 MinionGameStatePanel에 쿨타임 바 표시). 쿨타임 값·동작은 각 미니언 클래스")]
    public bool hasSkill;

    [Tooltip("wave 카드(MinionOfferCard)로 이 미니언을 놓을 때 드는 주사위 코인 (MinionData.csv Coin 열)")]
    public int offerDiceCoinPrice = GameSetting.OFFER_MINION_DICE_COIN_PRICE;

    // (2026-11-24) 로비 구매: 유저 레벨로 해금되면 이 재화·가격으로 구매 (MinionData.csv BuyCurrency·BuyPrice 열). 초기 지급 미니언은 안 씀
    [Tooltip("로비에서 이 미니언을 살 때 쓰는 재화 (MinionData.csv BuyCurrency 열)")]
    public CurrencyType buyCurrencyType = CurrencyType.Gold;
    [Tooltip("로비 구매 가격 (MinionData.csv BuyPrice 열)")]
    public int buyPrice;

    [Tooltip("인게임 직접 강화 1레벨당 공격력 증가(%). 종마다 다름 (MinionData.csv ReinforceAttackPower)")]
    public float reinforceAttackPowerPercent = 10f;

    [Tooltip("이 미니언의 공격 소리 (공격 모션과 같은 시점). AudioClip이 비면 SoundManager의 공통 소리 SFXType.MinionAttack. sfxType은 쓰지 않음")]
    public SFXData attackSfx;

    public Minion prefab;
    public ConditionData[] unlockConditions; // 추가 효과 해금 조건 (모두 충족해야 효과 활성화)

    public Sprite thum;

    [Header("5성 진화 (2026-11-24)")]
    [Tooltip("진화한 몸 그림 (LoadData가 2.Sprites/Minion/Growth2/{Key}에서 찾음, 없으면 원래 그림)")]
    public Sprite evolvedSprite;
    [Tooltip("진화 갈래 (보통 2개 → 5성이 되면 하나 선택, 1개면 바로 진화). 비우면 공용 임시 갈래 MinionEvolutionData.DefaultBranches")]
    public List<MinionEvolutionData> evolutions = new List<MinionEvolutionData>();
    [Tooltip("(2026-11-26) 진화 이름·설명 (MinionData.csv EvolutionName / EvolutionDesc, 설명 {0}… = MinionEvolutionSetting 수치)")]
    public string evolutionName;
    [TextArea] public string evolutionDesc;

    // (2026-11-26) 진화 갈래: MinionEvolutionSetting에 이 종 칸이 있으면 그 하나, 없으면 에셋 evolutions(예전 방식), 둘 다 없으면 진화 없음
    public List<MinionEvolutionData> GetEvolutions()
    {
        EvolutionConfig config = MinionEvolutionSetting.Instance.Get(key);
        if (config != null)
            return new List<MinionEvolutionData> { MinionEvolutionData.FromConfig(this, config) };
        return evolutions != null ? evolutions : new List<MinionEvolutionData>();
    }

    // UI에 노출할 스탯 종류와 순서. 에셋별 인스펙터에서 설정한다. (예: Flame은 FlameDuration은 빼고 FlameDPS까지만)
    public List<StatType> displayStatTypes;

    public virtual List<MinionStat> GetDisplayMinionStats()
    {
        var result = new List<MinionStat>();
        foreach (StatType statType in displayStatTypes)
        {
            MinionStat stat = GetMinionStat(statType);
            if (stat != null)
                result.Add(stat);
        }
        return result;
    }

    // 기본 스탯 (MinionData.csv의 AttackPower, AttackSpeed, AttackRange, CritChance 열, mergeLv별 값)
    // (2026-11-24) 미니언 강화(MinionEnhance csv·강화 레벨) 삭제 → 공격력도 MinionData.csv AttackPower 열
    public List<MinionStat> baseMinionStats = new List<MinionStat>();
    public MinionStat GetBaseStat(StatType statType) => baseMinionStats?.FirstOrDefault(e => e.statType == statType);

    // 미니언 고유 스탯 (mergeLv별 값). CSV가 아니라 각 미니언 데이터 클래스의 LoadData 재정의에서 SetUniqueStat으로 입력
    public List<MinionStat> uniqueMinionStats = new List<MinionStat>();
    public MinionStat GetUniqueStat(StatType statType) => uniqueMinionStats?.FirstOrDefault(e => e.statType == statType);

    // 기본 스탯(MinionData.csv) → 고유 스탯 순서로 찾음
    public virtual MinionStat GetMinionStat(StatType statType) => GetBaseStat(statType) ?? GetUniqueStat(statType);

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

#if UNITY_EDITOR
    // (2026-11-21) 미니언별 데이터 클래스(FlameMinionData 등)에 코드로 적은 값 = 기준. LoadData 때 에셋 값을 코드 값으로 덮어씀
    // 대상: MinionData보다 아래(자식) 클래스에서 선언한 직렬화 필드 중 값 타입·문자열·그 배열/리스트
    //   (CrossBuffMinionData의 buffOpType·buffValues 포함). 에셋 참조(프리팹·스프라이트 등)와 코드 값이 null인 필드는 건드리지 않음
    // 공용 필드(key·attackType·hasSkill·attackSfx·offerDiceCoinPrice 등, CSV·인스펙터로 정함)는 그대로
    // → 미니언 고유 수치는 코드에서만 고침 (인스펙터에서 고쳐도 다음 LoadData 때 코드 값으로 돌아감)
    public List<string> ApplyCodeDefaults()
    {
        var changed = new List<string>();
        System.Type type = GetType();
        if (type == typeof(MinionData) || type.IsAbstract)
            return changed;
        var fresh = CreateInstance(type);
        try
        {
            for (System.Type t = type; t != null && t != typeof(MinionData); t = t.BaseType)
            {
                foreach (var f in t.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly))
                {
                    bool serialized = (f.IsPublic && !f.IsNotSerialized) || f.IsDefined(typeof(SerializeField), false);
                    if (!serialized || !IsPlainDataType(f.FieldType))
                        continue;
                    object code = f.GetValue(fresh);
                    if (code == null)
                        continue;
                    object cur = f.GetValue(this);
                    if (SameValue(cur, code))
                        continue;
                    f.SetValue(this, CloneValue(code));
                    changed.Add($"{f.Name}: {Show(cur)} → {Show(code)}");
                }
            }
        }
        finally
        {
            DestroyImmediate(fresh);
        }
        if (changed.Count > 0)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[MinionData] {name} 코드 값 적용\n{string.Join("\n", changed)}");
        }
        return changed;
    }

    static bool IsPlainDataType(System.Type t)
    {
        if (t.IsArray)
            return IsPlainDataType(t.GetElementType());
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
            return IsPlainDataType(t.GetGenericArguments()[0]);
        return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(Vector2) || t == typeof(Vector3) || t == typeof(Color);
    }

    static bool SameValue(object a, object b)
    {
        if (a is System.Collections.IList la && b is System.Collections.IList lb)
        {
            if (la.Count != lb.Count)
                return false;
            for (int i = 0; i < la.Count; i++)
                if (!Equals(la[i], lb[i]))
                    return false;
            return true;
        }
        return Equals(a, b);
    }

    static object CloneValue(object v)
    {
        if (v is System.Array arr)
            return arr.Clone();
        if (v is System.Collections.IList list)
            return System.Activator.CreateInstance(v.GetType(), list);
        return v;
    }

    static string Show(object v) => v is System.Collections.IList l
        ? "{" + string.Join(",", System.Linq.Enumerable.Cast<object>(l)) + "}" : v?.ToString() ?? "null";
#endif

    public static MinionData GetMinionData(string key)
    {
        return MinionManager.Instance.GetMinionData(key);
    }




#if UNITY_EDITOR
    public virtual void LoadData()
    {
        // 새로 만든 에셋은 key가 비어 있음 → 파일명(= Key)으로 채운다 (Edit과 같은 규칙)
        if (string.IsNullOrEmpty(key))
            key = name;
        ApplyCodeDefaults();

        TableReader table = TableReader.Load("MinionData.csv");
        if (table == null) return;

        TableReader.Row row = table.Find("Key", key);
        if (row == null)
        {
            Debug.LogWarning($"[MinionData] CSV에서 '{key}' 를 찾지 못함");
            return;
        }
        displayStatTypes = new List<StatType> { StatType.AttackPower, StatType.AttackSpeed, StatType.AttackRange };
        if (table.HasColumn("Desc"))
            desc = row.Get("Desc");
        evolutionName = table.HasColumn("EvolutionName") ? row.Get("EvolutionName")?.Trim() : null;
        evolutionDesc = table.HasColumn("EvolutionDesc") ? row.Get("EvolutionDesc")?.Trim() : null;
        if (row.TryGetInt("Growth", out int lv))
            growth = lv;
        // 카드 소환 가격 (주사위 코인)
        if (row.TryGetInt("Coin", out int coin))
            offerDiceCoinPrice = coin;
        // 로비 구매 재화·가격 (비면 골드 0)
        buyCurrencyType = row.TryGetEnum("BuyCurrency", out CurrencyType buyCurrency) ? buyCurrency : CurrencyType.Gold;
        buyPrice = row.TryGetInt("BuyPrice", out int price) ? price : 0;
        if (table.HasColumn("AttackType"))
        {
            if (row.TryGetEnum("AttackType", out AttackType parsedAttackType))
                attackType = parsedAttackType;
            else
                Debug.LogWarning($"[MinionData] {key} AttackType '{row.Get("AttackType")}' 파싱 실패");
        }
        if (table.HasColumn("Tags"))
        {
            tags = MinionTags.Parse(row.Get("Tags"), out string tagError);
            if (tagError != null)
                Debug.LogWarning($"[MinionData] {key} Tags 모르는 이름: {tagError}");
        }
        // 쿨타임 스킬 여부 (MinionData.csv HasSkill: TRUE/FALSE) → 머리 위 쿨타임 바
        if (table.HasColumn("HasSkill"))
            hasSkill = string.Equals(row.Get("HasSkill")?.Trim(), "TRUE", System.StringComparison.OrdinalIgnoreCase);
        if (row.TryGetFloat("ReinforceAttackPower", out float reinforcePercent))
            reinforceAttackPowerPercent = reinforcePercent;

        // 기본 스탯 (MinionData.csv). AttackSpeed·AttackRange·CritChance는 값 하나 = 머지 레벨과 상관없이 고정
        // AttackPower는 값 하나면 머지 레벨에 비례 (표시 머지 Lv.1 = 값, Lv.2 = 2배 …, ToMergeLinear), "/"로 여러 값이면 mergeLv별
        uniqueMinionStats = new List<MinionStat>(); // 자식 LoadData에서 다시 채움
        baseMinionStats = new List<MinionStat>();
        foreach (StatType statType in new[] { StatType.AttackPower, StatType.AttackSpeed, StatType.AttackRange, StatType.CritChance })
        {
            string raw = table.HasColumn(statType.ToString()) ? row.Get(statType.ToString()) : null;
            if (string.IsNullOrWhiteSpace(raw))
            {
                if (statType == StatType.AttackPower)
                    Debug.LogWarning($"[MinionData] {key} MinionData.csv AttackPower 없음 → 공격력 0");
                continue;
            }
            if (statType == StatType.AttackPower)
                raw = ToMergeLinear(raw);
            baseMinionStats.Add(new MinionStat
            {
                statType = statType,
                values = raw.Split('/').Select(v => v.Trim()).ToArray(),
            });
        }

        prefab = FindPrefab();
        thum = FindAssetByName<Sprite>($"Assets/2.Sprites/Minion/Growth{growth}", key);
        evolvedSprite = FindAssetByName<Sprite>(EVOLVED_SPRITE_FOLDER, key, warn: false); // 아직 그림이 없는 종은 null

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[MinionData] {key} LoadData 완료");
    }

    // 값 하나면 머지 레벨에 비례: "기본/증가량" 선형 형식(값/값)으로. "/"·"~"가 있으면 그대로
    public static string ToMergeLinear(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || raw.Contains("/") || raw.Contains("~"))
            return raw;
        string v = raw.Trim();
        return $"{v}/{v}";
    }

    // 미니언 고유 스탯을 uniqueMinionStats에 설정 (같은 type이면 덮어씀). raw = "1/2/3" (mergeLv별 값, 2개면 기본/증가량)
    protected void SetUniqueStat(StatType type, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;
        MinionStat stat = new MinionStat { statType = type, values = raw.Split('/') };
        int index = uniqueMinionStats.FindIndex(s => s.statType == type);
        if (index >= 0) uniqueMinionStats[index] = stat;
        else uniqueMinionStats.Add(stat);
    }

    // 미니언 프리팹: 3.Prefabs/Minion 하위 어디든 {key}Minion.prefab (등급 없음. 예전 등급 폴더 C/B/A/S도 그대로 찾음). 새 프리팹은 루트에 생성
    const string PREFAB_ROOT = "Assets/3.Prefabs/Minion";
    string PrefabFolder => PREFAB_ROOT;

    Minion FindPrefab()
    {
        string fileName = $"{key}Minion";
        string expectedPath = $"{PrefabFolder}/{fileName}.prefab";
        Minion found;
        foreach (string guid in UnityEditor.AssetDatabase.FindAssets($"{fileName} t:Prefab", new[] { PREFAB_ROOT }))
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) != fileName)
                continue;

            found = UnityEditor.AssetDatabase.LoadAssetAtPath<Minion>(path);
            if (found != null)
                return found;
        }

        Debug.LogWarning($"[MinionData] {key} 프리팹 없음 : {expectedPath}");
        return null;
    }

    // folder 안에서 파일명이 fileName과 일치하는 T 타입 에셋을 찾는다.
    const string EVOLVED_SPRITE_FOLDER = "Assets/2.Sprites/Minion/Growth2";

    T FindAssetByName<T>(string folder, string fileName, bool warn = true) where T : UnityEngine.Object
    {
        if (!UnityEditor.AssetDatabase.IsValidFolder(folder))
        {
            if (warn)
                Debug.LogWarning($"[MinionData] 폴더 없음: {folder}");
            return null;
        }

        string typeFilter = typeof(T) == typeof(Minion) ? "t:Prefab" : "t:Sprite";
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"{fileName} {typeFilter}", new[] { folder });

        // 스프라이트: 같은 이름 중 .png 우선(작업용 .psd 제외), Multiple로 잘린 이미지면 가장 큰 조각(본체)
        if (typeof(T) == typeof(Sprite))
        {
            Sprite best = guids.Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                .Where(p => System.IO.Path.GetFileNameWithoutExtension(p) == fileName)
                .OrderBy(p => p.EndsWith(".psd", System.StringComparison.OrdinalIgnoreCase) ? 1 : 0)
                .Select(p => UnityEditor.AssetDatabase.LoadAllAssetsAtPath(p).OfType<Sprite>().OrderByDescending(s => s.rect.width * s.rect.height).FirstOrDefault())
                .FirstOrDefault(s => s != null);
            if (best != null)
                return best as T;
        }

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) != fileName) continue;

            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
        }

        if (warn)
            Debug.LogWarning($"[MinionData] {folder} 에서 '{fileName}' 를 찾지 못함");
        return null;
    }


    public void Edit()
    {
        if (string.IsNullOrEmpty(key) || key != name)
            key = name;

        string prefabRootFolder = PrefabFolder;
        string prefabPath = $"{prefabRootFolder}/{key}Minion.prefab";

        // 이미 프리팹이 있으면(하위 폴더 포함) 그대로 연결만 하고 덮어쓰지 않는다.
        Minion existingPrefab = FindPrefab();
        if (existingPrefab != null)
        {
            prefab = existingPrefab;
            prefab.key = key;
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[MinionData] {key} 기존 prefab 연결 완료: {UnityEditor.AssetDatabase.GetAssetPath(existingPrefab)}");
            return;
        }

        // 프리팹 없으면 루트 폴더에 새로 생성
        if (!UnityEditor.AssetDatabase.IsValidFolder(prefabRootFolder))
            System.IO.Directory.CreateDirectory(prefabRootFolder);

        var go = new GameObject(key + "Minion");

        // key+"Minion" 이름의 타입이 존재하면 컴포넌트 추가, 없으면 스크립트 파일 생성
        string componentTypeName = key + "Minion";
        System.Type minionType = System.Type.GetType(componentTypeName);
        if (minionType != null && typeof(Minion).IsAssignableFrom(minionType))
        {
            go.AddComponent(minionType);
            Debug.Log($"[MinionData] {componentTypeName} 컴포넌트 추가됨");
        }
        else
        {
            string scriptPath = CreateMinionScript(componentTypeName);
            Debug.LogWarning($"[MinionData] {componentTypeName} 타입 없음 → 스크립트 생성: {scriptPath}\n컴파일 완료 후 Edit을 다시 눌러주세요.");
            DestroyImmediate(go);
            UnityEditor.AssetDatabase.Refresh();
            return;
        }

        var newPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        DestroyImmediate(go);

        prefab = newPrefab.GetComponent<Minion>();
        prefab.key = key;
        newPrefab.AddComponent<MinionMovement>();
        Rigidbody2D rg2d = newPrefab.AddComponent<Rigidbody2D>();
        rg2d.freezeRotation = true;
        rg2d.gravityScale = 0;
        CircleCollider2D col = newPrefab.AddComponent<CircleCollider2D>();
        col.offset = new Vector2(0, 0.075f);
        col.radius = 0.4f;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[MinionData] {key} 새 prefab 생성 완료: {prefabRootFolder}");
    }

    string CreateMinionScript(string className)
    {
        string scriptFolder = $"Assets/1.Scripts/InGame/Minion/Unit/{className}";
        System.IO.Directory.CreateDirectory(scriptFolder);
        string filePath = $"{scriptFolder}/{className}.cs";

        if (System.IO.File.Exists(filePath))
            return filePath;

        string content =
$@"using UnityEngine;

public class {className} : Minion
{{

}}
";
        System.IO.File.WriteAllText(filePath, content);
        return filePath;
    }
#endif
}

[System.Serializable]
public class MinionStat
{
    public StatType statType;
    public string[] values;
    public string Title => StatData.GetTitle(statType);
    public Sprite Thum => StatData.GetThum(statType);

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
    // (2026-11-21) 공격 타입 4종 = 카오스·물리·마법·진동. 배율은 Resources/DefenceInfo 표 (카오스도 표 값 사용, 기본 전부 100%)
    Chaos, // 카오스 (정의할 수 없는 무언가). (2026-11-21 AllRounder → Chaos)
    Physic, //물리
    Magic, //마법
    Vibration, //진동 (2026-11-21 추가)
}
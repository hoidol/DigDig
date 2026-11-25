using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// (2026-11-24) 용사 데이터. Json/HeroData.csv (시트 gid 754105383) 한 행 = 용사 하나
// 킹 기본 스탯: 용사를 고르기 전 = KingSlimeData.csv, 용사를 고르면 = 이 표의 그 용사 행 (HeroKingAbility.OnEquip → KingSlime.ApplyHeroBaseStats)
// key = 용사 클래스 이름에서 Hero를 뺀 것 (KnightHero → Knight, HeroKingAbility.HeroDataKey)
// (2026-11-25) 레벨업마다 스탯 증가: 열 "Add{StatType}" (AddAttackPower 등) = 레벨당 더하는 값 → 용사 레벨 L의 스탯 = 기본 + Add × (L - 1)
//   정예병 칸 버프: EliteBuffDesc(설명, {0} = 값. 시트 이전 이름 "정예병 칸 버프"도 읽음) · InitBuffValue(Lv.1 값) · AddBuffValue(레벨당)
//   버프 종류(스탯·곱/더하기)는 용사 프리팹 eliteBuffs[0], 값만 이 표 (곱이면 % → 1 + 값/100)
// (2026-11-25) 스킬: Skill1·2·3 = 설명({0}{1}… 자리, 뒤에 %가 붙은 자리는 ×100으로 표시) · Skill1Value… = 자리마다 "기본/강화" 쉼표로 (예 "5/7,3/5")
//   강화 값 = 용사 강화 카드 "N단계 스킬 강화"를 골랐을 때 (/가 없으면 강화해도 같음). 3단계(공용)는 강화 없음
// hidden·Desc·BuyCurrency·BuyPrice·unnlockLv(시트 철자 그대로)는 나중에 로비 용사 기능용으로 읽어만 둠
// DataTableImporter가 CSV → Resources/HeroData.asset
public class HeroDataTable : ScriptableObject
{
    public const string RESOURCE_PATH = "HeroData";
    public const string CSV_PATH = "HeroData.csv";

    // 킹 스탯으로 쓰는 열 (열 이름 = StatType 이름)
    public static readonly StatType[] STAT_COLUMNS =
    {
        StatType.AttackPower, StatType.MaxHp, StatType.AttackSpeed, StatType.CritChance, StatType.CritPower, StatType.AttackRange,
    };

    [System.Serializable]
    public class HeroStat
    {
        public StatType statType;
        public float value;
    }

    [System.Serializable]
    public class HeroSkill
    {
        public string desc;
        public List<float> values = new List<float>();
        public List<float> enhancedValues = new List<float>();

        // "5/7,3/5" → values 5,3 / enhanced 7,5
        public static HeroSkill Parse(string desc, string valueText)
        {
            var s = new HeroSkill { desc = desc?.Trim() ?? "" };
            if (string.IsNullOrWhiteSpace(valueText))
                return s;
            foreach (string part in valueText.Split(','))
            {
                string[] pair = part.Split('/');
                float.TryParse(pair[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v);
                float e = v;
                if (pair.Length > 1)
                    float.TryParse(pair[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out e);
                s.values.Add(v);
                s.enhancedValues.Add(e);
            }
            return s;
        }
    }

    [System.Serializable]
    public class HeroData
    {
        public List<HeroSkill> skills = new List<HeroSkill>(); // index 0~2 = Skill1~3
        public string key;
        public List<HeroStat> stats = new List<HeroStat>();
        public List<HeroStat> addStats = new List<HeroStat>(); // 레벨당 증가
        public string eliteBuffDesc;
        public float eliteBuffInit;
        public float eliteBuffAdd;
        public bool HasEliteBuff => eliteBuffInit != 0f || eliteBuffAdd != 0f;
        public bool hidden;
        public string desc;
        public CurrencyType buyCurrency;
        public int buyPrice;
        public int unlockLv;

        public bool TryGetStat(StatType statType, out float value)
        {
            HeroStat s = stats.FirstOrDefault(e => e.statType == statType);
            value = s != null ? s.value : 0f;
            return s != null;
        }

        public float GetAdd(StatType statType)
        {
            HeroStat s = addStats.FirstOrDefault(e => e.statType == statType);
            return s != null ? s.value : 0f;
        }

        // 용사 레벨(1부터)의 스탯. 표에 없으면 false
        public bool TryGetStat(StatType statType, int heroLevel, out float value)
        {
            bool has = TryGetStat(statType, out value);
            value += GetAdd(statType) * Mathf.Max(0, heroLevel - 1);
            return has;
        }

        public float GetEliteBuffValue(int heroLevel) => eliteBuffInit + eliteBuffAdd * Mathf.Max(0, heroLevel - 1);
    }

    static HeroDataTable instance;
    public static HeroDataTable Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<HeroDataTable>(RESOURCE_PATH);
            return instance;
        }
    }

    public List<HeroData> heroDatas = new List<HeroData>();

    public static HeroData Get(string key)
    {
        var table = Instance;
        return table != null && !string.IsNullOrEmpty(key) ? table.heroDatas.FirstOrDefault(h => h.key == key) : null;
    }

#if UNITY_EDITOR
    const string ELITE_BUFF_DESC_COLUMN = "EliteBuffDesc";
    const string ELITE_BUFF_DESC_COLUMN_OLD = "정예병 칸 버프";
    const string ASSET_PATH = "Assets/Resources/" + RESOURCE_PATH + ".asset";

    // 에셋이 없으면 만든 뒤 CSV 반영 (DataTableImporter에서 호출)
    public static int ImportAsset()
    {
        var data = UnityEditor.AssetDatabase.LoadAssetAtPath<HeroDataTable>(ASSET_PATH);
        if (data == null)
        {
            data = CreateInstance<HeroDataTable>();
            UnityEditor.AssetDatabase.CreateAsset(data, ASSET_PATH);
        }
        data.LoadData();
        return 1;
    }

    public void LoadData()
    {
        TableReader table = TableReader.Load(CSV_PATH);
        if (table == null) return;

        string keyColumn = table.HasColumn("key") ? "key" : "Key";
        var datas = new List<HeroData>();
        foreach (TableReader.Row row in table.rows)
        {
            string key = row.Get(keyColumn)?.Trim();
            if (string.IsNullOrEmpty(key))
                continue;
            var data = new HeroData { key = key };
            foreach (StatType statType in STAT_COLUMNS)
            {
                if (row.TryGetFloat(statType.ToString(), out float v))
                    data.stats.Add(new HeroStat { statType = statType, value = v });
                if (row.TryGetFloat("Add" + statType, out float add) && add != 0f)
                    data.addStats.Add(new HeroStat { statType = statType, value = add });
            }
            string descColumn = table.HasColumn(ELITE_BUFF_DESC_COLUMN) ? ELITE_BUFF_DESC_COLUMN : ELITE_BUFF_DESC_COLUMN_OLD;
            data.eliteBuffDesc = table.HasColumn(descColumn) ? row.Get(descColumn)?.Trim() : "";
            data.eliteBuffInit = row.TryGetFloat("InitBuffValue", out float init) ? init : 0f;
            data.eliteBuffAdd = row.TryGetFloat("AddBuffValue", out float addBuff) ? addBuff : 0f;
            for (int i = 1; i <= 3; i++)
                data.skills.Add(HeroSkill.Parse(table.HasColumn($"Skill{i}") ? row.Get($"Skill{i}") : "",
                    table.HasColumn($"Skill{i}Value") ? row.Get($"Skill{i}Value") : ""));
            data.hidden = string.Equals(row.Get("hidden")?.Trim(), "TRUE", System.StringComparison.OrdinalIgnoreCase);
            data.desc = table.HasColumn("Desc") ? row.Get("Desc") : "";
            data.buyCurrency = row.TryGetEnum("BuyCurrency", out CurrencyType c) ? c : CurrencyType.Gold;
            data.buyPrice = row.TryGetInt("BuyPrice", out int price) ? price : 0;
            data.unlockLv = row.TryGetInt("unnlockLv", out int lv) || row.TryGetInt("unlockLv", out lv) ? lv : 0;
            datas.Add(data);
        }
        heroDatas = datas;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[HeroDataTable] LoadData 완료 ({datas.Count}개: {string.Join(", ", datas.Select(d => d.key))})");
    }
#endif
}

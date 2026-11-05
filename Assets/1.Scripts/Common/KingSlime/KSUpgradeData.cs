using System.Collections.Generic;
using UnityEngine;

// 킹슬라임 로비 업그레이드 (StatType별). Json/KSUpgradeData.csv 한 행 = StatType 하나.
// StatType이 추가되면 CSV에 행만 추가하면 된다. 행이 없는 StatType은 업그레이드 불가.
[CreateAssetMenu(fileName = "KSUpgradeData", menuName = "KSUpgradeData")]
public class KSUpgradeData : ScriptableObject
{
    public const string RESOURCE_PATH = "KingSlimeData/Lucky/KSUpgradeData";

    static KSUpgradeData instance;
    public static KSUpgradeData Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<KSUpgradeData>(RESOURCE_PATH);
            return instance;
        }
    }

    public List<KSUpgradeInfo> upgradeInfos = new List<KSUpgradeInfo>();

    public KSUpgradeInfo GetUpgradeInfo(StatType statType)
    {
        return upgradeInfos.Find(u => u.statType == statType);
    }

#if UNITY_EDITOR
    const string ASSET_PATH = "Assets/Resources/" + RESOURCE_PATH + ".asset";

    // 에셋이 없으면 만든 뒤 CSV 반영 (DataTableImporter에서 호출)
    public static int ImportAsset()
    {
        var data = UnityEditor.AssetDatabase.LoadAssetAtPath<KSUpgradeData>(ASSET_PATH);
        if (data == null)
        {
            data = CreateInstance<KSUpgradeData>();
            UnityEditor.AssetDatabase.CreateAsset(data, ASSET_PATH);
        }
        data.LoadData();
        return 1;
    }

    public void LoadData()
    {
        TableReader table = TableReader.Load("KSUpgradeData.csv");
        if (table == null) return;

        var infos = new List<KSUpgradeInfo>();
        foreach (TableReader.Row row in table.rows)
        {
            if (!row.TryGetEnum("StatType", out StatType statType))
            {
                Debug.LogWarning($"[KSUpgradeData] {row.lineNumber}줄 StatType '{row.Get("StatType")}' 없음 (무시)");
                continue;
            }

            var info = new KSUpgradeInfo { statType = statType };
            if (row.TryGetEnum("CurrencyType", out CurrencyType currencyType)) info.currencyType = currencyType;
            if (row.TryGetInt("MaxLv", out int maxLv)) info.maxLv = maxLv;
            if (row.TryGetFloat("ValuePerLv", out float valuePerLv)) info.valuePerLv = valuePerLv;
            if (row.TryGetInt("BasePrice", out int basePrice)) info.basePrice = basePrice;
            if (row.TryGetInt("PriceAddPerLv", out int priceAdd)) info.priceAddPerLv = priceAdd;
            if (row.TryGetFloat("PriceMulPerLv", out float priceMul)) info.priceMulPerLv = priceMul;
            infos.Add(info);
        }
        upgradeInfos = infos;

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[KSUpgradeData] LoadData 완료 ({upgradeInfos.Count}개)");
    }
#endif
}

[System.Serializable]
public class KSUpgradeInfo
{
    public StatType statType;
    public CurrencyType currencyType;
    public int maxLv; // 0이면 무제한
    public float valuePerLv; // lv당 더해지는 스탯
    // 가격(lv → lv+1) = (basePrice + priceAddPerLv × lv) × priceMulPerLv^lv
    public int basePrice;
    public int priceAddPerLv;
    public float priceMulPerLv = 1f;

    public bool IsMaxLv(int lv) => maxLv > 0 && lv >= maxLv;

    public float GetValue(int lv) => valuePerLv * lv;

    public int GetPrice(int lv)
    {
        return Mathf.RoundToInt((basePrice + priceAddPerLv * lv) * Mathf.Pow(priceMulPerLv, lv));
    }
}

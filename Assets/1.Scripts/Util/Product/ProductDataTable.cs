using System.Collections.Generic;
using UnityEngine;

// 상품 데이터. Json/ProductData.csv 한 행 = 상품 하나 (key, productId, currencyType, price)
// DataTableImporter가 CSV → Resources/ProductData.asset, ProductManager가 시작할 때 불러옴
public class ProductDataTable : ScriptableObject
{
    public const string RESOURCE_PATH = "ProductData";

    static ProductDataTable instance;
    public static ProductDataTable Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<ProductDataTable>(RESOURCE_PATH);
            return instance;
        }
    }

    public List<ProductData> productDatas = new List<ProductData>();

#if UNITY_EDITOR
    const string ASSET_PATH = "Assets/Resources/" + RESOURCE_PATH + ".asset";

    // 에셋이 없으면 만든 뒤 CSV 반영 (DataTableImporter에서 호출)
    public static int ImportAsset()
    {
        var data = UnityEditor.AssetDatabase.LoadAssetAtPath<ProductDataTable>(ASSET_PATH);
        if (data == null)
        {
            data = CreateInstance<ProductDataTable>();
            UnityEditor.AssetDatabase.CreateAsset(data, ASSET_PATH);
        }
        data.LoadData();
        return 1;
    }

    public void LoadData()
    {
        TableReader table = TableReader.Load("ProductData.csv");
        if (table == null) return;

        var datas = new List<ProductData>();
        foreach (TableReader.Row row in table.rows)
        {
            string key = row.Get("key");
            if (string.IsNullOrEmpty(key))
                continue;
            if (!row.TryGetEnum("currencyType", out CurrencyType currencyType))
            {
                Debug.LogWarning($"[ProductDataTable] {row.lineNumber}줄 {key} currencyType '{row.Get("currencyType")}' 없음 (무시)");
                continue;
            }

            var data = new ProductData { key = key, productId = row.Get("productId"), currencyType = currencyType };
            if (row.TryGetFloat("price", out float price)) data.price = price;
            if (currencyType == CurrencyType.InApp && string.IsNullOrEmpty(data.productId))
                Debug.LogWarning($"[ProductDataTable] {row.lineNumber}줄 {key} 인앱 상품인데 productId 없음 → 결제 불가");
            datas.Add(data);
        }
        productDatas = datas;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[ProductDataTable] LoadData 완료 ({datas.Count}개)");
    }
#endif
}

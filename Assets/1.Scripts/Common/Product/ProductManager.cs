using System;
using UnityEngine;

public class ProductManager : MonoSingleton<ProductManager>
{
    public ProductData[] productDatas;

    public ProductData GetProductData(string key)
    {
        for(int i = 0; i < productDatas.Length; i++)
        {
            if(productDatas[i].key == key)
            return productDatas[i];
        }
        return null;
    }

    public bool TryToBuy(ProductData productData, Action<bool> success)
    {
        if (!string.IsNullOrEmpty(productData.productId))
        {
            InAppPurchaseManager.Instance.Buy(productData.productId,success);
        }
        return true;
    }

}
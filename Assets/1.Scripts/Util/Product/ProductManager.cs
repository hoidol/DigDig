using System;
using UnityEngine;

public class ProductManager : MonoSingleton<ProductManager>
{
    public ProductData[] productDatas;

    public ProductData GetProductData(string key)
    {
        for (int i = 0; i < productDatas.Length; i++)
        {
            if (productDatas[i].key == key)
                return productDatas[i];
        }
        return null;
    }

    // 인앱은 결제 요청만 하고 결과는 success로 전달. 재화 상품은 잔액이 모자라면 false (차감 안 함)
    public void TryToBuy(ProductData productData, Action<bool> success)
    {
        if (!string.IsNullOrEmpty(productData.productId))
        {
            InAppPurchaseManager.Instance.Buy(productData.productId, success);
            return;
        }
        if(productData.currencyType == CurrencyType.Ad)
        {
            if (UserDataManager.Instance.isAdRemoved)
            {
                ToastCanvas.Toast("광고 제거 상품을 구매하셨습니다.");
                success?.Invoke(false);
                return;
            }
            AdManager.Instance.Play($"product_{productData.key}", AdType.RA, (adSuccess) =>
            {
                if (adSuccess)
                {
                    UserDataManager.Instance.userPurchaseManager.AddUserPurchase(productData);
                    success?.Invoke(true);
                }
                else
                {
                    success?.Invoke(false);
                }
            });
            return;
        }

        if (!UserDataManager.Instance.TryUseCurrency(productData.currencyType, productData.price))
        {
            ToastCanvas.Toast($"Not enough {productData.currencyType}");
            success?.Invoke(false);
            return;
        }

        success?.Invoke(true);
    }

}
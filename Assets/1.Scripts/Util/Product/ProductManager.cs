using System;
using UnityEngine;

public class ProductManager : MonoSingleton<ProductManager>
{
    public ProductData[] productDatas;

    // 테스트용: 켜면 인앱 결제창을 띄우지 않고 바로 결제 성공 (플레이 테스트가 켜고 끝나면 끔. 기본 꺼짐)
    public static bool SkipInAppPurchaseForTest;

    // ProductData.csv(ProductDataTable)가 있으면 그걸 씀 (인스펙터 값 무시)
    void Awake()
    {
        if (ProductDataTable.Instance != null)
            productDatas = ProductDataTable.Instance.productDatas.ToArray();
    }

    public ProductData GetProductData(string key)
    {
        for (int i = 0; i < productDatas.Length; i++)
        {
            if (productDatas[i].key == key)
                return productDatas[i];
        }
        return null;
    }

    // 인앱 = 결제 완료, 광고 = 시청 완료, 재화 = 잔액 확인 후 차감(모자라면 false, 차감 안 함) → success
    // 구매 기록은 여기서 남기지 않음 (ProductPanel.Purchased에서 한 번)
    public void TryToBuy(ProductData productData, Action<bool> success)
    {
        if (!string.IsNullOrEmpty(productData.productId))
        {
            if (SkipInAppPurchaseForTest)
            {
                Debug.Log($"[IAP] 테스트: 결제 생략 → 성공 ({productData.productId})");
                success?.Invoke(true);
                return;
            }
            if (InAppPurchaseManager.Instance == null) // 아직 StartSetting에서 생성 안 함
            {
                ToastCanvas.Toast(TranslateManager.GetText("Purchase_Unavailable"));
                success?.Invoke(false);
                return;
            }
            InAppPurchaseManager.Instance.Buy(productData.productId, success);
            return;
        }
        if(productData.currencyType == CurrencyType.Ad)
        {
            // 광고 제거 구매자는 광고 없이 바로 지급
            if (UserDataManager.Instance.isAdRemoved)
            {
                success?.Invoke(true);
                return;
            }
            AdManager.Show($"product_{productData.key}", AdType.RA, success);
            return;
        }

        if (!UserDataManager.Instance.TryUseCurrency(productData.currencyType, Mathf.RoundToInt(productData.price)))
        {
            ToastCanvas.Toast($"Not enough {productData.currencyType}");
            success?.Invoke(false);
            return;
        }

        success?.Invoke(true);
    }

}
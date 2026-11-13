using UnityEngine;
[System.Serializable]
public class ProductData
{
    public string key;
    public string productId; //인앱 결제용
    // 상품 설명 = 번역 시트 Product.{key}.Desc
    public static string GetDesc(string key) => TranslateManager.GetText($"Product.{key}.Desc");
    // public string adId; //광고용
    public CurrencyType currencyType;
    public float price; // 재화 = 개수, 인앱 = 스토어 가격을 못 받을 때 표시용(USD)
    public string GetPriceToString()
    {
        // 인앱 매니저가 아직 없으면(StartSetting에서 생성 안 함) price 그대로
        if (!string.IsNullOrEmpty(productId) && InAppPurchaseManager.Instance != null)
        {
            string formattedPrice = InAppPurchaseManager.Instance.GetFormattedPrice(productId);
            if (!string.IsNullOrEmpty(formattedPrice))
            {
                return formattedPrice;
            }
        }
        if (currencyType == CurrencyType.InApp)
            return $"${price:0.##}";
        return Mathf.RoundToInt(price).ToString();
    }
}

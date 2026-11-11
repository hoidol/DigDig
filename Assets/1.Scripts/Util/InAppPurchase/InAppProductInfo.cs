using System.Collections.Generic;
using UnityEngine.Purchasing;

// 인앱 결제 상품 정보를 한 곳에서 관리한다.
// key = ProductData.csv의 key(구매 기록·버튼 key), productId = 스토어(구글/애플)에 등록한 상품 ID.
// 인앱 상품을 추가하면 여기 PRODUCTS에 한 줄 추가 + ProductData.csv에 같은 key 행(currencyType = InApp, price = 표시용 USD) 추가.
// productId는 CSV가 아니라 여기 값이 쓰인다(ProductDataTable.LoadData).
public static class InAppProductInfo
{
    public class Info
    {
        public readonly string key;
        public readonly string productId;
        public readonly ProductType productType;

        public Info(string key, string productId, ProductType productType)
        {
            this.key = key;
            this.productId = productId;
            this.productType = productType;
        }
    }

    // 코드에서 구매 여부를 확인하는 상품 key
    public const string REMOVE_AD_KEY = "RemoveAd";
    public const string GAME_SPEED_UP_KEY = "GameSpeedUp"; // 구매하면 인게임 배속 5배까지

    public static readonly IReadOnlyList<Info> PRODUCTS = new List<Info>
    {
        // 비소모성 (구매 복원 대상)
        new Info(REMOVE_AD_KEY, "remove_ad", ProductType.NonConsumable),
        new Info(GAME_SPEED_UP_KEY, "game_speed_up", ProductType.NonConsumable),

        // 소모성
        new Info("Dia_20", "dia_20", ProductType.Consumable),
        new Info("Dia_110", "dia_110", ProductType.Consumable),
        new Info("Dia_550", "dia_550", ProductType.Consumable),
    };

    public static Info GetByKey(string key)
    {
        for (int i = 0; i < PRODUCTS.Count; i++)
            if (PRODUCTS[i].key == key) return PRODUCTS[i];
        return null;
    }

    public static Info GetByProductId(string productId)
    {
        for (int i = 0; i < PRODUCTS.Count; i++)
            if (PRODUCTS[i].productId == productId) return PRODUCTS[i];
        return null;
    }

    public static string GetProductId(string key) => GetByKey(key)?.productId;
}

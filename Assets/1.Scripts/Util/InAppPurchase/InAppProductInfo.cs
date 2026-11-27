using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing;

// 인앱 결제 상품 정보.
// (2026-11-27) 원본 = ProductData.csv (Key·ProductId·CurrencyType InApp·Price·ProductType) → 인앱 상품을 추가하면 CSV에 행만 추가.
// 여기엔 코드에서 구매 여부를 확인하는 key 상수만 둔다. PRODUCTS = ProductDataTable의 InApp 행
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

    // (2026-11-27) 판매 상품 4개: 광고 제거 · 게임 속도 · 기본 게임 팩 · 용사 패스 (스테이지 이용권·광고 제거/배속 패키지 삭제)
    public const string REMOVE_AD_KEY = "RemoveAd";
    public const string GAME_SPEED_UP_KEY = "GameSpeedUp"; // 구매하면 인게임 배속 5배까지
    // 기본 게임 팩 = 영구 광고 제거 + 게임 속도 + 용사 패스 BASE_GAME_PACK_HERO_DAYS일 (구매 시각부터, HeroPass)
    public const string BASE_GAME_PACK_KEY = "BaseGamePack";
    public const int BASE_GAME_PACK_HERO_DAYS = 30;
    // 용사 패스 (월 구독): 있으면 isFree가 아닌 용사도 모두 이용 (HeroPass)
    public const string HERO_PASS_KEY = "HeroPass";

    // 이 중 하나라도 샀으면 효과 적용
    public static readonly string[] REMOVE_AD_KEYS = { REMOVE_AD_KEY, BASE_GAME_PACK_KEY };
    public static readonly string[] GAME_SPEED_UP_KEYS = { GAME_SPEED_UP_KEY, BASE_GAME_PACK_KEY };

    public static IReadOnlyList<Info> PRODUCTS
    {
        get
        {
            ProductDataTable table = ProductDataTable.Instance;
            if (table == null)
                return new List<Info>();
            return table.productDatas
                .Where(d => d != null && d.currencyType == CurrencyType.InApp && !string.IsNullOrEmpty(d.productId))
                .Select(d => new Info(d.key, d.productId, d.productType))
                .ToList();
        }
    }

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

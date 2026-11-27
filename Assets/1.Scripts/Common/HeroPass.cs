using System;
using UnityEngine;

// (2026-11-27) 용사 패스 = 월 구독 (ProductData.csv HeroPass, ProductType Subscription)
// - 구독 중이면 HeroData.csv isFree가 아닌 용사도 모두 이용 (UserHeroManager.IsOwned). isFree 용사는 패스 없이 레벨 해금
// - 구독 상태는 스토어가 원본: 스토어 연결·앱 복귀·구매 직후 InAppPurchaseManager.CheckHeroPass → SetActive
// - 마지막으로 "구독 중"을 확인한 뒤 OFFLINE_GRACE_HOURS 동안은 오프라인이어도 구독 중으로 봄 (PlayerPrefs). 해지·만료는 다음 확인 때 반영
// - (2026-11-27) 기본 게임 팩(BaseGamePack)을 사면 구매 시각부터 BASE_GAME_PACK_HERO_DAYS일 동안도 이용 (구매 기록 purchaseTime 기준)
// - 서버 영수증 검증은 아직 없음 (나중에 Firebase Functions로 붙일 것)
public static class HeroPass
{
    public const double OFFLINE_GRACE_HOURS = 72;
    const string ACTIVE_UNTIL_KEY = "HeroPass_ActiveUntilUtc";

    // 테스트용: null이 아니면 이 값을 씀 (기본 null)
    public static bool? ForceActiveForTest;

    public static event Action OnChanged;

    public static bool IsActive
    {
        get
        {
            if (ForceActiveForTest.HasValue)
                return ForceActiveForTest.Value;
            return IsSubscribed || IsBundleActive;
        }
    }

    // 구독 중 (스토어 확인 결과, 오프라인 유예 포함)
    public static bool IsSubscribed => IsActiveAt(PlayerPrefs.GetString(ACTIVE_UNTIL_KEY, ""), DateTime.UtcNow);

    // 기본 게임 팩의 용사 패스 기간이 남음
    public static bool IsBundleActive
    {
        get
        {
            var purchase = UserDataManager.Instance != null ? UserDataManager.Instance.userPurchaseManager : null;
            if (purchase?.userPurchaseData == null)
                return false;
            foreach (UserPurchase p in purchase.userPurchaseData.userPurchases)
                if (p.key == InAppProductInfo.BASE_GAME_PACK_KEY && IsBundleActiveAt(p.purchaseTime, TimeManager.Now))
                    return true;
            return false;
        }
    }

    // 씬 없이 계산 (테스트): 구매 시각(TimeManager 형식) + BASE_GAME_PACK_HERO_DAYS일 > now
    public static bool IsBundleActiveAt(string purchaseTime, DateTime now)
    {
        if (string.IsNullOrEmpty(purchaseTime))
            return false;
        try
        {
            return now < TimeManager.StringToDateTime(purchaseTime).AddDays(InAppProductInfo.BASE_GAME_PACK_HERO_DAYS);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static void NotifyChanged() => OnChanged?.Invoke();

    // 씬 없이 계산 (테스트)
    public static bool IsActiveAt(string activeUntilUtcTicks, DateTime nowUtc) =>
        long.TryParse(activeUntilUtcTicks, out long ticks) && nowUtc.Ticks < ticks;

    // 스토어 확인 결과 반영 (구독 중 = 지금부터 OFFLINE_GRACE_HOURS까지 유효). 기본 게임 팩 기간은 따로 (구매 기록)
    public static void SetActive(bool active)
    {
        bool before = IsActive;
        if (active)
            PlayerPrefs.SetString(ACTIVE_UNTIL_KEY, DateTime.UtcNow.AddHours(OFFLINE_GRACE_HOURS).Ticks.ToString());
        else
            PlayerPrefs.DeleteKey(ACTIVE_UNTIL_KEY);
        PlayerPrefs.Save();
        if (before != IsActive)
            OnChanged?.Invoke();
    }

    public static ProductData Product =>
        ProductManager.Instance != null ? ProductManager.Instance.GetProductData(InAppProductInfo.HERO_PASS_KEY) : null;

    public static string PriceText
    {
        get
        {
            ProductData product = Product;
            return product != null ? product.GetPriceToString() : TranslateManager.GetText("Unlock_NotReady");
        }
    }

    // 구독 결제 → 성공하면 바로 구독 중 + 구매 기록. 결과는 메인 스레드에서 done
    public static void Buy(Action<bool> done)
    {
        ProductData product = Product;
        if (product == null || ProductManager.Instance == null)
        {
            ToastCanvas.Toast(TranslateManager.GetText("Unlock_NotReady"));
            done?.Invoke(false);
            return;
        }
        ProductManager.Instance.TryToBuy(product, async ok =>
        {
            await Cysharp.Threading.Tasks.UniTask.SwitchToMainThread(); // 결제 SDK 콜백은 메인 스레드가 아닐 수 있음
            if (ok)
                UserDataManager.Instance.userPurchaseManager.AddUserPurchase(product); // → SetActive(true)
            done?.Invoke(ok);
        });
    }
}

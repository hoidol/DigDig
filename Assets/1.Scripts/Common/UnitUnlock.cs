using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

// (2026-11-27) 미니언 해금 방식 (골드·다이아 구매 폐기). 용사는 isFree + 용사 패스 구독(HeroPass)으로 따로
// MinionData.csv BuyCurrency 열: 빈칸(또는 Auto) = 유저 레벨이 되면 자동 보유 / Ad = 레벨이 되면 광고를 보고 해금 / InApp = 레벨이 되면 유료 결제
// InApp 상품 = ProductData key "Minion_{key}" (ProductData.csv에 InApp 행 추가해야 결제 가능, 없으면 "준비 중" 토스트)
public enum UnitUnlockType
{
    Auto,
    Ad,
    InApp,
}

public static class UnitUnlock
{
    public static UnitUnlockType Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return UnitUnlockType.Auto;
        if (Enum.TryParse(value.Trim(), true, out UnitUnlockType type))
            return type;
        Debug.LogWarning($"[UnitUnlock] BuyCurrency 값 '{value}'을(를) 알 수 없음 → 자동 해금 (빈칸 / Ad / InApp)");
        return UnitUnlockType.Auto;
    }

    public static string MinionProductKey(string key) => $"Minion_{key}";

    static ProductData GetProduct(string productKey) =>
        ProductManager.Instance != null ? ProductManager.Instance.GetProductData(productKey) : null;

    // 버튼에 보일 글씨 (광고 = "광고", 유료 = 가격, 자동 = 무료)
    public static string GetPriceText(UnitUnlockType type, string productKey)
    {
        switch (type)
        {
            case UnitUnlockType.Ad:
                return TranslateManager.GetText("Ad");
            case UnitUnlockType.InApp:
                ProductData product = GetProduct(productKey);
                return product != null ? product.GetPriceToString() : TranslateManager.GetText("Unlock_NotReady");
            default:
                return TranslateManager.GetText("Shop_Free");
        }
    }

    public static Sprite GetIcon(UnitUnlockType type) =>
        type == UnitUnlockType.Ad ? CurrencyInfo.GetSprite(CurrencyType.Ad)
        : type == UnitUnlockType.InApp ? CurrencyInfo.GetSprite(CurrencyType.InApp)
        : null;

    // 광고 시청 / 결제 → 성공하면 done(true). 자동은 바로 성공. 결과는 메인 스레드에서
    public static void Pay(UnitUnlockType type, string productKey, Action<bool> done)
    {
        switch (type)
        {
            case UnitUnlockType.Ad:
                AdManager.Show($"unlock_{productKey}", AdType.RA, ok => OnMain(ok, done));
                return;
            case UnitUnlockType.InApp:
                ProductData product = GetProduct(productKey);
                if (product == null || ProductManager.Instance == null)
                {
                    ToastCanvas.Toast(TranslateManager.GetText("Unlock_NotReady"));
                    done?.Invoke(false);
                    return;
                }
                ProductManager.Instance.TryToBuy(product, ok =>
                {
                    if (ok)
                        UserDataManager.Instance.userPurchaseManager.AddUserPurchase(product);
                    OnMain(ok, done);
                });
                return;
            default:
                done?.Invoke(true);
                return;
        }
    }

    static async void OnMain(bool ok, Action<bool> done)
    {
        await UniTask.SwitchToMainThread(); // 광고·결제 SDK 콜백은 메인 스레드가 아닐 수 있음
        done?.Invoke(ok);
    }
}

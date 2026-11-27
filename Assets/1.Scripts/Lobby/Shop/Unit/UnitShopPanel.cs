using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-26) 상점 [용사]·[미니언] 칸 하나 (다이아 상품 칸 모양을 복사해 씀, UnitShopBuilder)
// 상태: 보유 = 가격 자리에 "보유"(버튼 흐리게) / 해금 가능 = 광고·유료 아이콘 + "광고"·가격 / 잠김 = "Lv.N"
// (2026-11-27) 골드·다이아 구매 폐기 → 누르면 해금 가능일 때만 확인 팝업 → 광고 시청·결제 (용사 UserHeroManager.UnlockHero / 미니언 UserMinionManager.UnlockMinion, UnitUnlock)
public class UnitShopPanel : ProductPanel
{
    public enum UnitKind { Hero, Minion }

    public UnitKind kind;
    public string key;
    public TMP_Text titleText;
    public Image iconImage;
    public Button buyButton;
    public CurrencyDisplayPanel priceDisplay;

    bool hooked;

    void Awake() => HookButton();

    void HookButton()
    {
        if (hooked || buyButton == null)
            return;
        hooked = true;
        buyButton.onClick.AddListener(OnClickedBuy);
    }

    public void Setup(UnitKind unitKind, string unitKey)
    {
        kind = unitKind;
        key = unitKey;
        HookButton();
        UpdatePanel();
    }

    // ---- 표시용 정보
    string Title
    {
        get
        {
            if (kind == UnitKind.Hero)
            {
                AbilityData card = HeroCollectionContainer.GetHeroCard(key);
                return card != null && !string.IsNullOrEmpty(card.title) ? card.title : key;
            }
            MinionData data = MinionManager.Instance != null ? MinionManager.Instance.GetMinionData(key) : null;
            return data != null ? data.Title : key;
        }
    }

    Sprite Icon
    {
        get
        {
            if (kind == UnitKind.Hero)
                return HeroCollectionContainer.GetHeroCard(key)?.thum;
            MinionData data = MinionManager.Instance != null ? MinionManager.Instance.GetMinionData(key) : null;
            return data != null ? data.thum : null;
        }
    }

    (UnitUnlockType type, string productKey, int unlockLv) UnlockInfo
    {
        get
        {
            if (kind == UnitKind.Hero)
            {
                // (2026-11-27) 패스 용사 = 용사 패스(구독, 유료) / 무료 용사 = 레벨 해금
                HeroDataTable.HeroData h = HeroDataTable.Get(key);
                bool free = h == null || h.isFree;
                return (free ? UnitUnlockType.Auto : UnitUnlockType.InApp, InAppProductInfo.HERO_PASS_KEY, h != null ? h.unlockLv : 0);
            }
            MinionData data = MinionManager.Instance != null ? MinionManager.Instance.GetMinionData(key) : null;
            return (data != null ? data.unlockType : UnitUnlockType.Auto, UnitUnlock.MinionProductKey(key), UserMinionManager.GetUnlockLevel(key));
        }
    }

    MinionBuyState State => kind == UnitKind.Hero
        ? UserDataManager.Instance.userHeroManager.GetBuyState(key)
        : UserDataManager.Instance.userMinionManager.GetBuyState(key);

    public override void UpdatePanel()
    {
        if (string.IsNullOrEmpty(key) || UserDataManager.Instance == null)
            return;
        if (titleText != null)
            titleText.text = Title;
        if (iconImage != null)
        {
            iconImage.sprite = Icon;
            iconImage.enabled = iconImage.sprite != null;
            iconImage.preserveAspect = true;
        }
        MinionBuyState state = State;
        var (type, productKey, unlockLv) = UnlockInfo;
        if (iconImage != null)
            iconImage.color = state == MinionBuyState.Locked ? new Color(0.25f, 0.25f, 0.3f, 1f) : Color.white;
        if (priceDisplay != null)
        {
            Sprite icon = state == MinionBuyState.Buyable ? UnitUnlock.GetIcon(type) : null;
            if (priceDisplay.currencyImage != null)
            {
                priceDisplay.currencyImage.gameObject.SetActive(icon != null);
                if (icon != null)
                    priceDisplay.currencyImage.sprite = icon;
            }
            if (priceDisplay.valueText != null)
            {
                priceDisplay.valueText.text = state == MinionBuyState.Owned ? TranslateManager.GetText("Shop_Owned")
                    : state == MinionBuyState.Locked ? $"Lv.{unlockLv}"
                    : kind == UnitKind.Hero && type == UnitUnlockType.InApp ? TranslateManager.GetText("HeroPass_Label")
                    : UnitUnlock.GetPriceText(type, productKey);
                priceDisplay.valueText.color = Color.white;
            }
        }
        if (buyButton != null)
            buyButton.interactable = state == MinionBuyState.Buyable;
    }

    public void OnClickedBuy()
    {
        MinionBuyState state = State;
        var (type, productKey, unlockLv) = UnlockInfo;
        if (state == MinionBuyState.Owned)
            return;
        if (state == MinionBuyState.Locked)
        {
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("Shop_UnlockLevel"), unlockLv));
            return;
        }
        if (kind == UnitKind.Hero && type == UnitUnlockType.InApp)
        {
            // 용사 패스: 사면 모든 패스 용사 이용
            YesOrNoCanvas.Instance.OpenCanvas(TranslateManager.GetText("HeroPass_Title"),
                string.Format(TranslateManager.GetText("HeroPass_Confirm"), HeroPass.PriceText), yes =>
            {
                if (yes)
                    Buy();
            });
            return;
        }
        string titleKey = kind == UnitKind.Hero ? "Hero_BuyConfirm_Title" : "Minion_BuyConfirm_Title";
        string body = string.Format(TranslateManager.GetText(UnitUnlockConfirmKey(type)), Title, UnitUnlock.GetPriceText(type, productKey));
        YesOrNoCanvas.Instance.OpenCanvas(TranslateManager.GetText(titleKey), body, yes =>
        {
            if (yes)
                Buy();
        });
    }

    // 확인 팝업 글씨: 광고 = "광고를 보고 해금할까요?" / 유료 = "가격에 구매할까요?"
    public static string UnitUnlockConfirmKey(UnitUnlockType type) =>
        type == UnitUnlockType.Ad ? "Unlock_Confirm_Ad" : "Unlock_Confirm_InApp";

    // 확인 없이 바로 해금 (테스트·확인 팝업 Yes). 광고·결제 결과는 done으로
    public void Buy(System.Action<MinionBuyResult> done = null)
    {
        System.Action<MinionBuyResult> onResult = result =>
        {
            if (result == MinionBuyResult.Success)
            {
                SoundManager.Play(SFXType.PurchaseComplete);
                bool pass = kind == UnitKind.Hero && UnlockInfo.type == UnitUnlockType.InApp;
                ToastCanvas.Toast(pass ? TranslateManager.GetText("HeroPass_Bought") : string.Format(TranslateManager.GetText("Minion_Bought"), Title));
            }
            if (this != null)
            {
                ShopCanvas shop = GetComponentInParent<ShopCanvas>(true);
                if (shop != null)
                    shop.UpdateCanvas();
                else
                    UpdatePanel();
            }
            done?.Invoke(result);
        };
        if (kind == UnitKind.Hero)
            UserDataManager.Instance.userHeroManager.UnlockHero(key, onResult);
        else
            UserDataManager.Instance.userMinionManager.UnlockMinion(key, onResult);
    }
}

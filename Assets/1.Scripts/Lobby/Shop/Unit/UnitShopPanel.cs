using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-26) 상점 [용사]·[미니언] 칸 하나 (다이아 상품 칸 모양을 복사해 씀, UnitShopBuilder)
// 상태: 보유 = 가격 자리에 "보유"(버튼 흐리게) / 구매 가능 = 재화 아이콘 + 가격 (모자라면 빨간 숫자) / 잠김 = "Lv.N"
// 누르면 구매 가능일 때만 확인 팝업 → 구매 (용사 UserHeroManager.BuyHero / 미니언 UserMinionManager.BuyMinion)
public class UnitShopPanel : ProductPanel
{
    public enum UnitKind { Hero, Minion }

    public UnitKind kind;
    public string key;
    public TMP_Text titleText;
    public Image iconImage;
    public Button buyButton;
    public CurrencyDisplayPanel priceDisplay;

    static readonly Color NOT_ENOUGH = new Color(1f, 0.35f, 0.35f);

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

    (CurrencyType currency, int price, int unlockLv) BuyInfo
    {
        get
        {
            if (kind == UnitKind.Hero)
            {
                HeroDataTable.HeroData h = HeroDataTable.Get(key);
                return h != null ? (h.buyCurrency, h.buyPrice, h.unlockLv) : (CurrencyType.Gold, 0, 0);
            }
            MinionData data = MinionManager.Instance != null ? MinionManager.Instance.GetMinionData(key) : null;
            return (data != null ? data.buyCurrencyType : CurrencyType.Gold, data != null ? data.buyPrice : 0, UserMinionManager.GetUnlockLevel(key));
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
        var (currency, price, unlockLv) = BuyInfo;
        if (iconImage != null)
            iconImage.color = state == MinionBuyState.Locked ? new Color(0.25f, 0.25f, 0.3f, 1f) : Color.white;
        if (priceDisplay != null)
        {
            bool showCurrency = state == MinionBuyState.Buyable && price > 0;
            if (priceDisplay.currencyImage != null)
            {
                priceDisplay.currencyImage.gameObject.SetActive(showCurrency);
                if (showCurrency)
                    priceDisplay.currencyImage.sprite = CurrencyInfo.GetSprite(currency);
            }
            if (priceDisplay.valueText != null)
            {
                priceDisplay.valueText.text = state == MinionBuyState.Owned ? TranslateManager.GetText("Shop_Owned")
                    : state == MinionBuyState.Locked ? $"Lv.{unlockLv}"
                    : price > 0 ? price.ToString("N0") : TranslateManager.GetText("Shop_Free");
                priceDisplay.valueText.color = state == MinionBuyState.Buyable && price > UserDataManager.Instance.userData.GetCurrency(currency) ? NOT_ENOUGH : Color.white;
            }
        }
        if (buyButton != null)
            buyButton.interactable = state == MinionBuyState.Buyable;
    }

    public void OnClickedBuy()
    {
        MinionBuyState state = State;
        var (currency, price, unlockLv) = BuyInfo;
        if (state == MinionBuyState.Owned)
            return;
        if (state == MinionBuyState.Locked)
        {
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("Shop_UnlockLevel"), unlockLv));
            return;
        }
        string titleKey = kind == UnitKind.Hero ? "Hero_BuyConfirm_Title" : "Minion_BuyConfirm_Title";
        string body = string.Format(TranslateManager.GetText("Minion_BuyConfirm_Body"), Title, TranslateManager.GetText($"Currency.{currency}"), price);
        YesOrNoCanvas.Instance.OpenCanvas(TranslateManager.GetText(titleKey), body, yes =>
        {
            if (yes)
                Buy();
        });
    }

    // 확인 없이 바로 구매 (테스트·확인 팝업 Yes)
    public MinionBuyResult Buy()
    {
        var (currency, _, _) = BuyInfo;
        MinionBuyResult result = kind == UnitKind.Hero
            ? UserDataManager.Instance.userHeroManager.BuyHero(key)
            : UserDataManager.Instance.userMinionManager.BuyMinion(key);
        if (result == MinionBuyResult.NotEnoughCurrency)
        {
            SoundManager.Play(SFXType.Fail_NotEnough);
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("Currency_NotEnough"), TranslateManager.GetText($"Currency.{currency}")));
        }
        else if (result == MinionBuyResult.Success)
        {
            SoundManager.Play(SFXType.PurchaseComplete);
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("Minion_Bought"), Title));
            CurrencyPanel.UpdatePanels(currency);
        }
        ShopCanvas shop = GetComponentInParent<ShopCanvas>(true);
        if (shop != null)
            shop.UpdateCanvas();
        else
            UpdatePanel();
        return result;
    }
}

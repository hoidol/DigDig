using System.Linq;
using TMPro;
using UnityEngine;

// 한 번만 사는 인앱 상품 칸 (2026-11-27 판매 상품 = 광고 제거·게임 속도·기본 게임 팩 + 용사 패스(HeroPassPanel)). 구매하면 패널을 숨긴다
// 구매 여부 = 하위 버튼 key의 구매 기록 (광고 제거·게임 속도 칸은 효과를 이미 가졌는지, 기본 게임 팩으로 얻은 경우 포함)
// rewards = 구매 시 추가 지급 재화(에너지), 구매 버튼 onPurchased에 Purchased 등록
public class PackageProductPanel : ProductPanel
{
    [System.Serializable]
    public class Reward
    {
        public CurrencyType currencyType; // Energe
        public int amount;
    }

    [Header("구매 시 지급 (광고 제거·배속 같은 구매 효과는 상품 key로 적용)")]
    public Reward[] rewards;
    public TMP_Text descText; // 상품 설명 (번역 시트 Product.{버튼 key}.Desc)

    // 구매 완료 토스트 (Translate key)
    protected virtual string PurchasedMessageKey => "Purchase_Package";

    public virtual bool IsPurchased
    {
        get
        {
            if (purchaseButtons == null)
                purchaseButtons = GetComponentsInChildren<ProductPurchaseButton>(true);
            return purchaseButtons.Any(b => !string.IsNullOrEmpty(b.key) && UserDataManager.Instance.userPurchaseManager.HasPurchased(b.key));
        }
    }

    public override void UpdatePanel()
    {
        base.UpdatePanel();
        if (IsPurchased)
        {
            gameObject.SetActive(false);
            return;
        }
        if (descText != null && purchaseButtons.Length > 0)
            descText.text = ProductData.GetDesc(purchaseButtons[0].key);
    }

    public override void Purchased()
    {
        base.Purchased();
        if (rewards != null)
        {
            foreach (Reward reward in rewards)
                GiveReward(reward);
        }
        ToastCanvas.Toast(TranslateManager.GetText(PurchasedMessageKey));
        gameObject.SetActive(false);
    }

    void GiveReward(Reward reward)
    {
        UserDataManager user = UserDataManager.Instance;
        switch (reward.currencyType)
        {
            case CurrencyType.Energe: user.AddEnerge(reward.amount); break;
            default:
                Debug.LogError($"[{name}] 패키지로 지급할 수 없는 재화 : {reward.currencyType}");
                return;
        }
        CurrencyPanel.UpdatePanels(reward.currencyType);
    }
}

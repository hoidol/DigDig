using TMPro;
using UnityEngine;

// (2026-11-27) 용사 패스 칸 (버튼 key = HeroPass, 월 구독): 구독 중이면 숨기지 않고 "구독 중" + 구매 버튼 잠금
// 구독 상태 = HeroPass.IsSubscribed (기본 게임 팩의 30일은 따로라 구독 버튼은 그대로 보임)
public class HeroPassPanel : PackageProductPanel
{
    public GameObject subscribedObject; // 구독 중일 때 켜짐 (선택)
    public TMP_Text statusText;         // "구독 중" (선택)

    public override bool IsPurchased => false; // 구독이라 숨기지 않음

    void OnEnable() => HeroPass.OnChanged += UpdatePanel;
    void OnDisable() => HeroPass.OnChanged -= UpdatePanel;

    public override void UpdatePanel()
    {
        base.UpdatePanel();
        bool subscribed = HeroPass.IsSubscribed;
        if (subscribedObject != null)
            subscribedObject.SetActive(subscribed);
        if (statusText != null)
        {
            statusText.gameObject.SetActive(subscribed);
            statusText.text = TranslateManager.GetText("HeroPass_Subscribed");
        }
        if (purchaseButtons != null)
            foreach (ProductPurchaseButton b in purchaseButtons)
                if (b != null && b.TryGetComponent(out UnityEngine.UI.Button button))
                    button.interactable = !subscribed;
    }

    public override void Purchased()
    {
        // 구독 시작은 UserPurchaseManager.AddUserPurchase → HeroPass.SetActive. 패널은 그대로 두고 상태만 갱신
        ToastCanvas.Toast(TranslateManager.GetText("HeroPass_Bought"));
        UpdatePanel();
    }
}

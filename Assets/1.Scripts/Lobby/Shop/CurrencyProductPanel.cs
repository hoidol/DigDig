using TMPro;
using UnityEngine;

// 재화 상품 공용 (다이아·골드): 구매 버튼 onPurchased → Purchased → amount만큼 지급. 제목은 amount로 자동
public abstract class CurrencyProductPanel : ProductPanel
{
    public int amount; // 지급 개수
    public TMP_Text titleText;

    public abstract CurrencyType RewardCurrencyType { get; }
    protected abstract string CurrencyName { get; }

    public override void UpdatePanel()
    {
        base.UpdatePanel();
        if (titleText != null)
            titleText.text = $"{CurrencyName} {amount:N0}";
    }

    public override void Purchased()
    {
        base.Purchased();
        if (RewardCurrencyType == CurrencyType.Dia)
            UserDataManager.Instance.AddDia(amount);
        else if (RewardCurrencyType == CurrencyType.Gold)
            UserDataManager.Instance.AddGold(amount);
        CurrencyPanel.UpdatePanels(RewardCurrencyType);
        ToastCanvas.Toast(string.Format(TranslateManager.GetText("Currency_Get"), CurrencyName, amount));
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PurchaseExpPanel : UpgradePanel
{
    public TMP_Text expText;
    public Image expImage;
    public override void UpdatePanel()
    {
        Init();

        priceText.text = upgradeData.GetPrice().ToString();
        float expAmount = Player.Instance.exp / Player.Instance.GetMaxExp();
        expImage.fillAmount = expAmount;
        expText.text = $"{Player.Instance.exp}/{Player.Instance.GetMaxExp()}";
    }

}

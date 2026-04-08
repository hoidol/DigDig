using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UpgradePanel : MonoBehaviour
{
    public UpgradeType upgradeType;
    public TMP_Text titleText;
    public TMP_Text valueText;
    public TMP_Text priceText;
    public Button purchaseButton;
    protected UpgradeData upgradeData;

    bool init = false;
    protected virtual void Init()
    {
        if (init)
            return;
        init = true;
        titleText = transform.Find("TitleText")?.GetComponent<TMP_Text>();
        valueText = transform.Find("ValueText")?.GetComponent<TMP_Text>();
        purchaseButton = transform.Find("PurchaseButton")?.GetComponent<Button>();
        purchaseButton.onClick.AddListener(OnClickedUpgrade);
        priceText = purchaseButton.transform.Find("PriceText").GetComponent<TMP_Text>();
        if (upgradeData == null)
        {
            upgradeData = UpgradeData.GetUpgradeData(upgradeType);
        }

        titleText.text = upgradeData.Title;

    }

    public virtual void UpdatePanel()
    {
        Init();
        valueText.text = upgradeData.Value;
        priceText.text = upgradeData.GetPrice().ToString();
    }

    public virtual void OnClickedUpgrade()
    {

        if (upgradeData.GetPrice() > Player.Instance.gold)
        {
            //구매 불가
            return;
        }

        Player.Instance.AddGold(-upgradeData.GetPrice());
        //Player.Instance.Upgrade(upgradeType);
        UpgradeCanvas.Instance.UpdateCanvas();
    }
}
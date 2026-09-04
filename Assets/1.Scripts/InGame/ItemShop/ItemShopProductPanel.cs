using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemShopProductPanel : ItemPanel
{
    public bool purchased;
    
    public TMP_Text priceText;

    public GameObject purchasButtonObject;
    public GameObject purchasedObject;
    public override void SetItemData(ItemData itemData)
    {
        base.SetItemData(itemData);
        purchased= false;
        UpdatePanel();
    }

    public void UpdatePanel()
    {
        purchasButtonObject.SetActive(!purchased);
        purchasedObject.SetActive(purchased); 
        priceText.text= (ItemShopManager.Instance.GetPrice() + itemData.addPrice).ToString();
    }

    public void OnClickedPurchaseButton()
    {
        if(Character.Instance.coin < ItemShopManager.Instance.GetPrice())
        {
            ToastCanvas.Toast(TranslateManager.GetText("Not enough coin"));
            return;
        }
        bool success = Character.Instance.AddItem(itemData.key);
        if (success)
        {
            purchased = true;
            Character.Instance.AddCoin(-ItemShopManager.Instance.GetPrice());
        }
        GameEventBus.Publish(new PurchaseItemEvent(itemData));
        GetComponentInParent<ItemShopCanvas>().UpdateCanvas();
    }
}
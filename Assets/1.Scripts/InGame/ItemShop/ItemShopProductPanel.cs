using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemShopProductPanel : ItemPanel
{
    public bool purchased;
    
    public TMP_Text priceText;
    public GameObject purchasedObject;
    public override void SetItemData(ItemData itemData)
    {
        base.SetItemData(itemData);
        purchased= false;
        UpdatePanel();
    }

    public void UpdatePanel()
    {
        purchasedObject.SetActive(purchased); 
        priceText.text= (ItemShopManager.Instance.GetPrice() + itemData.addPrice).ToString();
    }

    public void OnClickedButton()
    {
        purchased = true;
        Character.Instance.AddOrePiece(-ItemShopManager.Instance.GetPrice());
        GameEventBus.Publish(new PurchaseItemEvent(itemData));
        GetComponentInParent<ItemShopCanvas>().UpdateCanvas();
    }
}
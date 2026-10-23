using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemProductPanel : ItemPanel
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
        // priceText.text= (ItemShopManager.Instance.GetPrice() + itemData.addPrice).ToString();
    }

    public void OnClickedInfo()
    {
        GetComponentInParent<ItemShopCanvas>().ShowItemInfo(this);
    }

    public async Task OnClickedPurchaseButton()
    {
        if(Character.Instance.coin < ItemShopManager.Instance.GetPrice())
        {
            ToastCanvas.Toast(TranslateManager.GetText("Not enough coin"));
            return;
        }
        PurchaseAsync().Forget();
    }

    public async UniTask<bool> PurchaseAsync()
    {
        bool success = await Character.Instance.AddItem(itemData.key);
        if (success)
        {
            purchased = true;
            Character.Instance.AddCoin(-ItemShopManager.Instance.GetPrice());
        }
        GameEventBus.Publish(new PurchaseItemEvent(itemData));
        GetComponentInParent<ItemShopCanvas>().UpdateCanvas();
        return success;
    }
}
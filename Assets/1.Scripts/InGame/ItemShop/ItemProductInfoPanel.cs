using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemProductInfoPanel : ItemPanel
{   
    public TMP_Text priceText;
    Action closeCallback;
    ItemProductPanel itemProductPanel;
    public GameObject purchaseButton;
    public void ShowItemInfo(ItemProductPanel iPPanel, Action cCallback)
    {
        itemProductPanel = iPPanel;
        purchaseButton.SetActive(!itemProductPanel.purchased);
        closeCallback = cCallback;
        SetItemData(itemProductPanel.itemData);
    }
    public override void SetItemData(ItemData itemData)
    {
        base.SetItemData(itemData);
 
        UpdatePanel();
    }

    public void UpdatePanel()
    {
        // priceText.text= (ItemShopManager.Instance.GetPrice() + itemData.addPrice).ToString();
    }

    bool isPurchasing; // 구매 처리 중 중복 클릭 방지

    // 버튼 OnClick 연결용 (UnityEvent는 void 메서드로 연결)
    public void OnClickedPurchaseButton()
    {
        PurchaseAsync().Forget();
    }

    async UniTaskVoid PurchaseAsync()
    {
        if (isPurchasing)
            return;

        if(Character.Instance.coin < ItemShopManager.Instance.GetPrice())
        {
            ToastCanvas.Toast(TranslateManager.GetText("Not enough coin"));
            return;
        }

        isPurchasing = true;
        bool success;
        try
        {
            success = await itemProductPanel.PurchaseAsync();
        }
        finally
        {
            isPurchasing = false;
        }

        //성공하면 닫기
        if (success)
            OnClickedClose();
    }
    
  

    public void OnClickedClose()
    {
        closeCallback.Invoke();
    }
}
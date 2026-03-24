using System;
using UnityEngine;

public class ItemCanvas : CanvasUI<ItemCanvas>
{
    public ItemPurchasePanel itemPurchasePanel;
    public ItemPickupPanel itemPickupPanel;
    ItemData curItemData;

    public void OpenCanvas(TryPurchaseItemEvent e, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        curItemData = e.itemData;
        itemPurchasePanel.SetItemData(curItemData);
        itemPurchasePanel.gameObject.SetActive(true);
        itemPickupPanel.gameObject.SetActive(false);
        //아이템 구매 관련
    }
    public void OpenCanvas(NearDropItemEvent e, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        curItemData = e.itemData;

        itemPurchasePanel.gameObject.SetActive(false);


        itemPickupPanel.SetItemData(curItemData);
        itemPickupPanel.gameObject.SetActive(true);
        //아이템 구매 관련
    }
    public void CloseCanvas(string key)
    {
        if (key != curItemData.key)
            return;
        base.CloseCanvas();

    }
}

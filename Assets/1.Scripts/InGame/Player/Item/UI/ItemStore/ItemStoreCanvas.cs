using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class ItemStoreCanvas : CanvasUI<ItemStoreCanvas>
{
    public ItemPurchasePanel[] itemPurchasePanels;
    Transform storeOwner;
    public void OpenCanvas(Transform storeOwner, Grade maxGrade, int count, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        this.storeOwner = storeOwner;

        Time.timeScale = 0;
        if (itemPurchasePanels == null || itemPurchasePanels.Length <= 0)
        {
            itemPurchasePanels = GetComponentsInChildren<ItemPurchasePanel>();

        }

        List<ItemData> itemDatas = ItemManager.Instance.GetItems(maxGrade, count);
        for (int i = 0; i < itemPurchasePanels.Length; i++)
        {
            if (i < itemDatas.Count)
            {
                itemPurchasePanels[i].SetItemData(itemDatas[i]);
                itemPurchasePanels[i].gameObject.SetActive(true);
            }
            else
            {
                itemPurchasePanels[i].gameObject.SetActive(false);
            }

        }
        // curItemData = e.itemData;
        // SetItemData(curItemData, r);
        //아이템 구매 관련
    }

    public void CloseCanvas(Transform sOwner)
    {
        if (storeOwner != sOwner)
            return;
        CloseCanvas();
    }
    public override void CloseCanvas()
    {
        base.CloseCanvas();
        Time.timeScale = 1;
    }
}

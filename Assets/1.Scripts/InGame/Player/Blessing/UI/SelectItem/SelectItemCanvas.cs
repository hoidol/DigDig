using System;
using System.Collections.Generic;
using UnityEngine;

//아이템 하나 반드시 선택해야됌
public class SelectItemCanvas : CanvasUI<SelectItemCanvas>
{
    public SelectItemPanel[] selectItemPanels;
    public override void OpenCanvas(Action closeCallback)
    {
        base.OpenCanvas(closeCallback);

        Time.timeScale = 0;
        if (selectItemPanels == null || selectItemPanels.Length <= 0)
        {
            selectItemPanels = GetComponentsInChildren<SelectItemPanel>();

        }
        UpdateCanvas();
    }

    public void UpdateCanvas()
    {
        List<ItemData> items = ItemManager.Instance.GetDrawItems(3);
        for (int i = 0; i < selectItemPanels.Length; i++)
        {
            selectItemPanels[i].SetItemData(items[i]);
        }
    }

    public void Selected(ItemData itemData)
    {
        Player.Instance.itemInventory.AddItem(itemData);
        CloseCanvas();
    }

    public override void CloseCanvas()
    {
        base.CloseCanvas();
        Time.timeScale = 1;
    }
}

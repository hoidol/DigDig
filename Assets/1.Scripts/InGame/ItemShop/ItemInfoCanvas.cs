using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoCanvas : CanvasUI<ItemInfoCanvas>
{
    //강화 시 능력치
    public ItemPanel itemPanel;

    Item curItem;
    int idx;
    public  void OpenCanvas(Item item,int idx, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        curItem = item;
        this.idx= idx;
        itemPanel.SetItemData(curItem.itemData);
        UpdateCanvas();
    }
    
    public void UpdateCanvas()
    {

    }

    public void OnClickedDiscard()
    {
        Character.Instance.RemoveItem(curItem.key,idx);
    }


}

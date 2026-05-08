using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
public class ItemPurchasePanel : ItemPanel
{
    public TMP_Text priceText;

    public override void SetItemData(ItemData itemData)
    {
        base.SetItemData(itemData);
        priceText.text = itemData.GetPrice().ToString();
    }
    public void OnClickedPurchase()
    {
        if (itemData.GetPrice() > Player.Instance.gold)
        {
            //구매 불가
            return;
        }
        GameEventBus.Publish<TryAddItemEvent>(new TryAddItemEvent(itemData));
        //ItemCanvas.Instance.CloseCanvas();
    }
}

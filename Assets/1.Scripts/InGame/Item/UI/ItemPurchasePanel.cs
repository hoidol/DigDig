using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
public class ItemPurchasePanel : ItemPickupPanel
{
    public TMP_Text priceText;

    public override void SetItemData(ItemData itemData, Action<bool> r)
    {
        base.SetItemData(itemData, r);
        priceText.text = itemData.GetPrice().ToString();
    }
    public override void OnClickedGet()
    {
        if (itemData.GetPrice() > Player.Instance.gold)
        {
            //구매 불가
            return;
        }
        GameEventBus.Publish<TryAddItemEvent>(new TryAddItemEvent(itemData));
        ItemCanvas.Instance.CloseCanvas();
    }
}

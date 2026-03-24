using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ItemPurchasePanel : ItemPickupPanel
{
    public TMP_Text priceText;

    public override void SetItemData(ItemData itemData)
    {
        base.SetItemData(itemData);
        priceText.text = itemData.GetPrice().ToString();
    }
    public override void OnClickedGet()
    {
        if (itemData.GetPrice() > Player.Instance.gold)
        {
            //구매 불가
            return;
        }
        GameEventBus.Publish<AddedItemEvent>(new AddedItemEvent(itemData));
        ItemCanvas.Instance.CloseCanvas();
    }
}

using TMPro;
using UnityEngine;

public class DrawItemButton : EnhanceStoneButton
{
    public  void UpdateButton()
    {
        titleText.text= TranslateManager.GetText($"DrawItem_Title") + " " + Character.Instance.itemInventory.curItems.Count + $"/{ItemInventory.MAX_ITEM_COUNT}";
    }
    public override void OnClickedBtn()
    {
        SelectItemCanvas.Instance.OpenCanvas(() =>
        {
        });

    }
}
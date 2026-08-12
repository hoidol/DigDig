
using UnityEngine;

public class ItemShopButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        ItemShopCanvas.Instance.OpenCanvas();
    }
}
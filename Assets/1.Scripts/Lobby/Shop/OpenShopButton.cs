using UnityEngine;

public class OpenShopButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        ShopCanvas.Instance.OpenCanvas();
    }
}
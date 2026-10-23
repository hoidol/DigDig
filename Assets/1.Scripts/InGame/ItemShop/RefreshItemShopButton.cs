using UnityEngine;

public class RefreshItemShopButton : ButtonUI
{
    int price = 5;
    public override void OnClickedBtn()
    {
        if(Character.Instance.coin < price)
        {
            ToastCanvas.Toast(TranslateManager.GetText("Not enough coin"));
            return;
        }
        ItemShopCanvas.Instance.ResetItemShopProduct();
    }

}

using UnityEngine;

public class PurchaseExpButton : ButtonUI
{
    public int price = 5;
    
    public override void OnClickedBtn()
    {
        if (Character.Instance.coin < price)
        {
            Debug.Log("Not enough coin");
            ToastCanvas.Toast("Not enough coin");
            return;
        }
        Character.Instance.AddCoin(-price);
        Character.Instance.AddExp(1);
    }
}
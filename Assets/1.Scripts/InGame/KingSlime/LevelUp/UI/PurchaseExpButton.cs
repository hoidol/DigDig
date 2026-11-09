using UnityEngine;

public class PurchaseExpButton : ButtonUI
{
    public int price = 5;
    
    public override void OnClickedBtn()
    {
        if (KingSlime.Instance.coin < price)
        {
            Debug.Log("Not enough coin");
            ToastCanvas.Toast("Not enough coin");
            return;
        }
        KingSlime.Instance.AddCoin(-price);
        // KingSlime.Instance.AddExp(1);
    }
}
using UnityEngine;

public class ShopButton : LobbyStateButton
{
    public override void OnClickedBtn()
    {
        LobbyManager.Instance.OpenCanvas(LobbyState.Shop);
    }
}
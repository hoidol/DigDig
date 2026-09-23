using UnityEngine;
using Lobby;
public class ShopButton : LobbyStateButton
{
    public override void OnClickedBtn()
    {
        LobbyManager.Instance.OpenCanvas(LobbyState.Shop);
    }
}
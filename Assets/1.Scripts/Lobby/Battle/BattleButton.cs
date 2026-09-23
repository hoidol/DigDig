using UnityEngine;
using Lobby;
public class BattleButton : LobbyStateButton
{
    public override void OnClickedBtn()
    {
        LobbyManager.Instance.OpenCanvas(LobbyState.Battle);
    }
}
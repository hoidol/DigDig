using UnityEngine;
using Lobby;
public class KSManagementButton : LobbyStateButton
{
    public override void OnClickedBtn()
    {
        LobbyManager.Instance.OpenCanvas(LobbyState.KingSlimeManagement);
    }

}

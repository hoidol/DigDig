using UnityEngine;

namespace Lobby
{
    public class SlimeManageButton : LobbyStateButton
    {
        public override void OnClickedBtn()
        {
            LobbyManager.Instance.OpenCanvas(LobbyState.Slime);
        }
    }    
}

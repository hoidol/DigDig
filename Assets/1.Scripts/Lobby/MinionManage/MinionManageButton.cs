using UnityEngine;

namespace Lobby
{
    public class MinionManageButton : LobbyStateButton
    {
        public override void OnClickedBtn()
        {
            LobbyManager.Instance.OpenCanvas(LobbyState.Minion);
        }
    }    
}

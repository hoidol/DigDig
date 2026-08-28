using UnityEngine;

namespace Lobby
{
    public class MiniMeManageButton : ButtonUI
    {
        public override void OnClickedBtn()
        {
            MiniMeManageCanvas.Instance.OpenCanvas();
        }
    }    
}

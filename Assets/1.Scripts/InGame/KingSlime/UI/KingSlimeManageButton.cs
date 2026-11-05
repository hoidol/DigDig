using TMPro;
using UnityEngine;
namespace InGame
{
    public class KingSlimeManageButton : ButtonUI
    {
        public override void OnClickedBtn()
        {
            KingSlimeManageCanvas.Instance.OpenCanvas();
        }
    }

}

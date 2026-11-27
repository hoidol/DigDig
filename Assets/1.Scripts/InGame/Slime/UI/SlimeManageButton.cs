using TMPro;
using UnityEngine;
namespace InGame
{
    public class SlimeManageButton : ButtonUI
    {
        public override void OnClickedBtn()
        {
            SlimeManageCanvas.Instance.OpenCanvas();
        }
    }

}

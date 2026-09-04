
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lobby
{
    public class SlimeEquipPanel : SlimePanel
    {
        public int idx;
        public void OnClickedPanel()
        {
            SlimeEquipCanvas.Instance.Selected(idx);
        }

    }    
}

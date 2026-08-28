
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lobby
{
    public class MiniMeEquipPanel : MiniMePanel
    {
        public int idx;
        public void OnClickedPanel()
        {
            MiniMeEquipCanvas.Instance.Selected(idx);
        }

    }    
}

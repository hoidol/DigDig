
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lobby
{
    public class MinionEquipPanel : MinionPanel
    {
        public int idx;
        public void OnClickedPanel()
        {
            MinionEquipCanvas.Instance.Selected(idx);
        }

    }    
}

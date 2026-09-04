using UnityEngine;
using UnityEngine.UI;
namespace Lobby
{
    public class SlimeEquipedSlotPanel : MonoBehaviour
    {
        public Image thumImage;
        public int idx;
        public void SetData(UserSlime userSlime)
        {
            thumImage.sprite= userSlime.SlimeData.thum;
        }

        public void UpdatePanel()
        {
            
        }
    }    
}

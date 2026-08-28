using UnityEngine;
using UnityEngine.UI;
namespace Lobby
{
    public class MiniMeEquipedSlotPanel : MonoBehaviour
    {
        public Image thumImage;
        public int idx;
        public void SetData(UserMiniMe userMiniMe)
        {
            thumImage.sprite= userMiniMe.MiniMeData.thum;
        }

        public void UpdatePanel()
        {
            
        }
    }    
}

using UnityEngine;
using UnityEngine.UI;
namespace Lobby
{
    public class SlimeEquipedSlotPanel : MonoBehaviour
    {
        public Image thumImage;
        public int idx;
        [SerializeField] UserSlime userSlime;
        public void SetData(UserSlime userSlime)
        {
            this.userSlime = userSlime;
            thumImage.sprite= userSlime.SlimeData.thum;
        }

        public void UpdatePanel()
        {
            
        }
    }    
}

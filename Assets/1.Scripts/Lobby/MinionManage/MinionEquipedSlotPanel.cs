using UnityEngine;
using UnityEngine.UI;
namespace Lobby
{
    public class MinionEquipedSlotPanel : MinionPanel
    {
        public Image thumImage;
        public int idx;
        [SerializeField] UserMinion userMinion;
        public void SetData(UserMinion userMinion)
        {
            this.userMinion = userMinion;
            thumImage.sprite= userMinion.MinionData.thum;
            SetData(userMinion.MinionData);
        }
         

    }    
}

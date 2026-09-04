using UnityEngine;
namespace Lobby
{
    public class SlimeEntryPanel : MonoBehaviour
    {
        public SlimePanel slimePanel;
        public SlimeData slimeData;
        public  UserSlime userSlime;
        public virtual void SetData(SlimeData slimeData)
        {
            this.slimeData = slimeData;
            userSlime = UserManager.Instance.userSlimeManager.GetUserSlime(slimeData.key); 
            slimePanel.SetData(slimeData);
        }

        public virtual void UpdatePanel()
        {
            if(slimeData ==null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
        }
        
        public virtual void OnClickedPanel()
        {
            SlimeInfoCanvas.Instance.OpenCanvas(slimeData);
        }
        
    }    
}

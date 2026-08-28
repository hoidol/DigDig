using UnityEngine;
namespace Lobby
{
    public class MiniMeEntryPanel : MonoBehaviour
    {
        public MiniMePanel miniMePanel;
        public MiniMeData miniMeData;
        public  UserMiniMe userMiniMe;
        public virtual void SetData(MiniMeData miniData)
        {
            this.miniMeData = miniData;
            userMiniMe = UserManager.Instance.userMiniMeManager.GetUserMiniMe(miniMeData.key); 
            miniMePanel.SetData(miniData);
        }

        public virtual void UpdatePanel()
        {
            if(miniMeData ==null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
        }
        
        public virtual void OnClickedPanel()
        {
            MiniMeInfoCanvas.Instance.OpenCanvas(miniMeData);
        }
        
    }    
}

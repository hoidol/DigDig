using UnityEngine;
namespace Lobby
{
    public class MinionEntryPanel : MonoBehaviour
    {
        public MinionPanel minionPanel;
        public MinionData minionData;
        public  UserMinion userMinion;
        public virtual void SetData(MinionData minionData)
        {
            this.minionData = minionData;
            if(minionData == null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
            userMinion = UserDataManager.Instance.userMinionManager.GetUserMinion(minionData.key); 
            if(minionPanel == null)
            {
                Debug.Log("MinionEntryPanel if(minionPanel == null)");
            }
            minionPanel.SetData(minionData);
        }

        public virtual void UpdatePanel()
        {
            if(minionData ==null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
        }
        
        public virtual void OnClickedPanel()
        {
            MinionInfoCanvas.Instance.OpenCanvas(minionData);
        }
        
    }    
}

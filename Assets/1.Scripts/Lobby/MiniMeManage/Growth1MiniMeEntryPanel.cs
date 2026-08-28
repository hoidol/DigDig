using UnityEngine;
namespace Lobby
{
    public class Growth1MiniMeEntryPanel : MiniMeEntryPanel
    {
        public GameObject equipingPanel;
        public GameObject equipButton;
        public GameObject unownPanel;

        public override void UpdatePanel()
        {
            if(miniMeData ==null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            equipingPanel.SetActive(false);
            equipButton.SetActive(false);
            unownPanel.SetActive(false);

            if(userMiniMe.own)
            {
                if(userMiniMe.Equiping)
                {
                    equipingPanel.SetActive(true);
                }
                else
                {       
                    equipButton.SetActive(true);
                }
            }
            else
            {
                unownPanel.SetActive(true);
            }

        }
        public virtual void OnClickedEquip()
        {
            MiniMeEquipCanvas.Instance.OpenCanvas(userMiniMe, () =>
            {
                MiniMeManageCanvas.Instance.UpdateCanvas();
            });
        }
    }    
}

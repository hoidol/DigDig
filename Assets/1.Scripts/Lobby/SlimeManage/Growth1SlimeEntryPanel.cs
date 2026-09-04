using UnityEngine;
namespace Lobby
{
    public class Growth1SlimeEntryPanel : SlimeEntryPanel
    {
        public GameObject equipingPanel;
        public GameObject equipButton;
        public GameObject unownPanel;

        public override void UpdatePanel()
        {
            if(slimeData ==null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            equipingPanel.SetActive(false);
            equipButton.SetActive(false);
            unownPanel.SetActive(false);

            if(userSlime.own)
            {
                if(userSlime.Equiping)
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
            SlimeEquipCanvas.Instance.OpenCanvas(userSlime, () =>
            {
                LobbyManager.Instance.GetLobbyCanvas(LobbyState.Slime).UpdateCanvas();
            });
        }
    }    
}

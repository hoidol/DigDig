using System;
using UnityEngine;

namespace Lobby
{
    public class SlimeInfoCanvas : CanvasUI<SlimeInfoCanvas>
    {
        public SlimePanel slimePanel;
        public GameObject equiptButton;
        public GameObject levelUpButton;
        public SlimeAbilityPanel[] slimeAbilityPanels;
        SlimeData slimeData;
        UserSlime userSlime;
        public void OpenCanvas(SlimeData slimeData, Action closeCallback = null)
        {
            this.slimeData = slimeData;
            this.userSlime = UserManager.Instance.userSlimeManager.GetUserSlime(slimeData.key);
            base.OpenCanvas(closeCallback);
            slimePanel.SetData(slimeData);
            OpenCanvas();
        }

        public void OpenCanvas()
        {
            UpdateCanvas();
        }

        public void UpdateCanvas()
        {
            equiptButton.SetActive(false);    
            if (userSlime.own)
            {
                equiptButton.SetActive(true);    
            }

            for(int i = 0; i < slimeAbilityPanels.Length; i++)
            {
                slimeAbilityPanels[i].SetSlimeData(slimeData);
            }
            
        }

        public void OnClickedLeft()
        {
            
        }

        public void OnClickedRight()
        {
            
        }
        public void OnClickedLevelUp()
        {
            
        }
        public void OnClickedEquipt()
        {
            SlimeEquipCanvas.Instance.OpenCanvas(userSlime, () =>
            {
                LobbyManager.Instance.GetLobbyCanvas(LobbyState.Slime).UpdateCanvas();
            });
        }
    }
}

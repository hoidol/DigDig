using System;
using UnityEngine;

namespace Lobby
{
    public class SlimeInfoCanvas : CanvasUI<SlimeInfoCanvas>
    {
        public SlimePanel slimePanel;
        public GameObject equiptButton;
        public GameObject levelUpButton;
        public SlimeMergeInfoPanel slimeMergeInfoPanel;
        public SlimeEnhanceAbilityPanel[] enhanceAbilityPanels;
        // public SlimeGradePanel slimeGradePanel;
        SlimeData slimeData;
        UserSlime userSlime;
        public void OpenCanvas(SlimeData slimeData, Action closeCallback = null)
        {
            this.slimeData = slimeData;
            this.userSlime = UserDataManager.Instance.userSlimeManager.GetUserSlime(slimeData.key);
            slimeMergeInfoPanel.SetSlimeData(userSlime,slimeData);
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

            slimeMergeInfoPanel.UpdatePanel();

            for (int i = 0; i < enhanceAbilityPanels.Length; i++)
            {
                enhanceAbilityPanels[i].SetSlimeData(slimeData, userSlime);
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
            int exp = SlimeManager.Instance.GetEnhanceExpInfo(userSlime.enhanceLevel, slimeData.grade);
            if( userSlime.exp < exp)
            {
                ToastCanvas.Toast(TranslateManager.GetText("Not enough exp"));
                return;
            }

            UserDataManager.Instance.userSlimeManager.LevelUp(slimeData.key);
            UpdateCanvas();
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

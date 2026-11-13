using System;
using UnityEngine;

namespace Lobby
{
    public class MinionInfoCanvas : CanvasUI<MinionInfoCanvas>
    {
        public MinionPanel minionPanel;
        public GameObject equiptButton;
        public GameObject levelUpButton;
        public MinionMergeInfoPanel minionMergeInfoPanel;

        MinionData minionData;
        UserMinion userMinion;
        public void OpenCanvas(MinionData minionData, Action closeCallback = null)
        {
            this.minionData = minionData;
            this.userMinion = UserDataManager.Instance.userMinionManager.GetUserMinion(minionData.key);
            minionMergeInfoPanel.SetMinionData(userMinion, minionData);
            base.OpenCanvas(closeCallback);
            minionPanel.SetData(minionData);
            OpenCanvas();
        }

        public void OpenCanvas()
        {
            UpdateCanvas();
        }

        public void UpdateCanvas()
        {
            // 보유했고 아직 장착 안 한 슬라임만 장착 버튼 노출
            equiptButton.SetActive(userMinion.own && !userMinion.Equiping);

            minionMergeInfoPanel.UpdatePanel();
        }

        public void OnClickedLeft()
        {

        }

        public void OnClickedRight()
        {

        }

        public void OnClickedLevelUp()
        {
            int exp = MinionManager.Instance.GetEnhanceExpInfo(userMinion.enhanceLevel, minionData.grade);
            if (userMinion.exp < exp)
            {
                ToastCanvas.Toast(TranslateManager.GetText("Not enough exp"));
                return;
            }

            UserDataManager.Instance.userMinionManager.LevelUp(minionData.key);
            UpdateCanvas();
        }
        public void OnClickedEquipt()
        {
            MinionEquipCanvas.Instance.OpenCanvas(userMinion, () =>
            {
                LobbyManager.Instance.GetLobbyCanvas(LobbyState.Minion).UpdateCanvas();
            });
        }
    }
}

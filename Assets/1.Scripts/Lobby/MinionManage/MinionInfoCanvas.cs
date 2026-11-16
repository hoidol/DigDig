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
            // 이번 레벨 필요 경험치를 소모하고 레벨업 (최대 레벨·경험치 부족이면 안내만)
            if (!UserDataManager.Instance.userMinionManager.LevelUp(minionData.key))
            {
                ToastCanvas.Toast(TranslateManager.GetText("Not enough exp"));
                return;
            }
            minionPanel.SetData(minionData);
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

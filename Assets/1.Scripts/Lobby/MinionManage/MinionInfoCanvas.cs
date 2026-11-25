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
            if (levelUpButton != null)
                levelUpButton.SetActive(false); // (2026-11-24) 미니언 강화 삭제 → 씬의 강화 버튼은 숨김 (씬에서 지워도 됨)

            minionMergeInfoPanel.UpdatePanel();
        }

        public void OnClickedLeft()
        {

        }

        public void OnClickedRight()
        {

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

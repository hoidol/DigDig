using System;
using UnityEngine;

namespace Lobby
{
    // 장착 창: 고른 미니언(minionPanel)을 장착 슬롯 5칸 중 누른 칸에 장착
    public class MinionEquipCanvas : CanvasUI<MinionEquipCanvas>
    {
        public MinionPanel minionPanel;
        public MinionEquipedSlotPanel[] slotPanels; // 장착 슬롯 순서대로 (비워 두면 하위 MinionEquipedSlotPanel 전부)

        UserMinion userMinion;

        public void OpenCanvas(UserMinion userMinion, Action closeCallback = null)
        {
            base.OpenCanvas(closeCallback);
            this.userMinion = userMinion;
            if (slotPanels == null || slotPanels.Length == 0)
                slotPanels = GetComponentsInChildren<MinionEquipedSlotPanel>(true);

            UserMinion[] equipped = UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions;
            for (int i = 0; i < slotPanels.Length; i++)
            {
                slotPanels[i].idx = i;
                bool has = i < equipped.Length && equipped[i] != null;
                slotPanels[i].gameObject.SetActive(has);
                if (has)
                    slotPanels[i].SetData(equipped[i]);
            }
            minionPanel.SetData(userMinion.MinionData);
        }

        public void Selected(int idx)
        {
            UserDataManager.Instance.userMinionManager.EquiptUserMinion(userMinion, idx);
            CloseCanvas();
        }
    }
}

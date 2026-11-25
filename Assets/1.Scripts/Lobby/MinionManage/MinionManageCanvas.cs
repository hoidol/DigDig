using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lobby
{
    public class MinionManageCanvas : BaseLobbyCanvas
    {
        public MinionEquipedSlotPanel[] equipedSlotPanels;
        [UnityEngine.Serialization.FormerlySerializedAs("entryPanels")]
        public MinionEntryPanel[] entryPanels;
        public Transform ownContainer; // 슬라임 칸 전부 (보유·미보유 한 컨테이너, 2026-11-19 unownContainer 삭제)
        public MinionPresetButton[] presetButtons;

        public override void Init()
        {
            if (init)
                return;
            init = true;

            // 칸은 SetData에서 켜고 끄므로 꺼져 있는 칸도 포함 (안 그러면 켜진 칸만 잡혀 슬라임이 일부만 보임)
            equipedSlotPanels = GetComponentsInChildren<MinionEquipedSlotPanel>(true);
            entryPanels = GetComponentsInChildren<MinionEntryPanel>(true);
            presetButtons = GetComponentsInChildren<MinionPresetButton>(true);
        }

        public override void OpenCanvas(Action closeCallback = null)
        {
            base.OpenCanvas(closeCallback);
            Init();
            OpenCanvas();
        }

        public void OpenCanvas()
        {
            for (int i = 0; i < equipedSlotPanels.Length; i++)
            {
                equipedSlotPanels[i].idx = i;
            }

            // 칸에 넣는 순서 = 지급 순서 (GameSetting.GetMinionOrder). 화면 순서(보유 먼저)는 UpdateCanvas에서 정렬
            MinionData[] minionDatas = MinionManager.Instance.allMinionDatas
                .Where(e => GameSetting.IsUsedMinion(e.key)) // 게임에서 쓰는 미니언만 (초기 지급 + 레벨 지급)
                .OrderBy(e => GameSetting.GetMinionOrder(e.key))
                .ToArray();

            EnsureEntryPanels(minionDatas.Length);
            for (int i = 0; i < entryPanels.Length; i++)
            {
                if (i < minionDatas.Length)
                {
                    entryPanels[i].SetData(minionDatas[i]);
                }
                else
                {
                    entryPanels[i].SetData(null);
                }
            }

            UpdateCanvas();
        }

        // 씬에는 슬라임 칸을 1개만 두고, 슬라임 수만큼 복사해서 쓴다 (슬라임이 늘어도 씬 수정 없음)
        void EnsureEntryPanels(int count)
        {
            if (entryPanels.Length == 0 || entryPanels.Length >= count)
                return;
            MinionEntryPanel template = entryPanels[0];
            var panels = new List<MinionEntryPanel>(entryPanels);
            while (panels.Count < count)
                panels.Add(Instantiate(template, template.transform.parent));
            entryPanels = panels.ToArray();
        }

        public override void UpdateCanvas()
        {
            for (int i = 0; i < UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions.Length; i++)
            {
                equipedSlotPanels[i].SetData(UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions[i]);
            }

            for (int i = 0; i < presetButtons.Length; i++)
            {
                presetButtons[i].UpdateButton();
            }

            for (int i = 0; i < entryPanels.Length; i++)
            {
                entryPanels[i].UpdatePanel();
            }

            // 화면 순서: 보유 → 구매 가능 → 잠김 (같은 상태는 지급 순서). 산 미니언은 다음 갱신 때 보유 쪽으로 옮겨짐
            var sortedPanels = entryPanels
                .Where(p => p.minionData != null)
                .OrderBy(p => (int)p.BuyState)
                .ThenBy(p => GameSetting.GetMinionOrder(p.minionData.key));
            foreach (MinionEntryPanel panel in sortedPanels)
            {
                if (panel.transform.parent != ownContainer)
                    panel.transform.SetParent(ownContainer, false);
                panel.transform.SetAsLastSibling();
            }

        }
    }
}

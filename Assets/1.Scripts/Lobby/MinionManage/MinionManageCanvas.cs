using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lobby
{
    public class MinionManageCanvas : BaseLobbyCanvas
    {
        public MinionEquipedSlotPanel[] equipedSlotPanels;
        public Growth1MinionEntryPanel[] growth1MinionEntryPanels;
        public Transform ownContainer; // 슬라임 칸 전부 (보유·미보유 한 컨테이너, 2026-11-19 unownContainer 삭제)
        public MinionPresetButton[] presetButtons;
        // public Growth2MinionEntryPanel[] growth2MinionEntryPanels;

        public override void Init()
        {
            if (init)
                return;
            init = true;

            // 칸은 SetData에서 켜고 끄므로 꺼져 있는 칸도 포함 (안 그러면 켜진 칸만 잡혀 슬라임이 일부만 보임)
            equipedSlotPanels = GetComponentsInChildren<MinionEquipedSlotPanel>(true);
            growth1MinionEntryPanels = GetComponentsInChildren<Growth1MinionEntryPanel>(true);
            presetButtons = GetComponentsInChildren<MinionPresetButton>(true);
            // growth2MinionEntryPanels = GetComponentsInChildren<Growth2MinionEntryPanel>();
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

            // 칸에 넣는 순서 = 등급 낮은 순 (C → B → A → S). 화면 순서(보유 먼저)는 UpdateCanvas에서 정렬
            MinionData[] minionDatas = MinionManager.Instance.allMinionDatas
                .Where(e => GameSetting.IsUsedMinion(e.key)) // 게임에서 쓰는 미니언만 (초기 지급 + 레벨 지급)
                .OrderBy(e => e.grade)
                .ToArray();

            EnsureEntryPanels(minionDatas.Length);
            for (int i = 0; i < growth1MinionEntryPanels.Length; i++)
            {
                if (i < minionDatas.Length)
                {
                    growth1MinionEntryPanels[i].SetData(minionDatas[i]);
                }
                else
                {
                    growth1MinionEntryPanels[i].SetData(null);
                }
            }

            // for (int i = 0; i < growth2MinionEntryPanels.Length; i++)
            // {
            //     if (i < MinionManager.Instance.growth2MinionDatas.Length)
            //     {
            //         growth2MinionEntryPanels[i].SetData(MinionManager.Instance.growth2MinionDatas[i]);
            //     }
            //     else
            //     {
            //         growth2MinionEntryPanels[i].SetData(null);
            //     }
            // }
            UpdateCanvas();
        }

        // 씬에는 슬라임 칸을 1개만 두고, 슬라임 수만큼 복사해서 쓴다 (슬라임이 늘어도 씬 수정 없음)
        void EnsureEntryPanels(int count)
        {
            if (growth1MinionEntryPanels.Length == 0 || growth1MinionEntryPanels.Length >= count)
                return;
            Growth1MinionEntryPanel template = growth1MinionEntryPanels[0];
            var panels = new List<Growth1MinionEntryPanel>(growth1MinionEntryPanels);
            while (panels.Count < count)
                panels.Add(Instantiate(template, template.transform.parent));
            growth1MinionEntryPanels = panels.ToArray();
        }

        public override void UpdateCanvas()
        {
            // List<string> equiptedMinionKeys = new List<string>();
            for (int i = 0; i < UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions.Length; i++)
            {
                equipedSlotPanels[i].SetData(UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions[i]);
                // equiptedMinionKeys.Add(UserManager.Instance.userMinionManager.userMinionData.equiptedMinions[i].key);
            }

            // List<string> canPickGrowth2Minions = new List<string>();
            // for(int i =0;i < MinionManager.Instance.growth2MinionDatas.Length; i++)
            // {
            //     Growth2MinionMergeData growth2MinionConditionData = MinionManager.Instance.GetGrowth2MinionMergeData(MinionManager.Instance.growth2MinionDatas[i].key); 

            //     if(growth2MinionConditionData.growth1MinionKeys.Length == 1)
            //     {
            //         if (equiptedMinionKeys.Contains(growth2MinionConditionData.growth1MinionKeys[0]))
            //         {
            //             canPickGrowth2Minions.Add(growth2MinionConditionData.key);
            //         }
            //     }
            //     else if(growth2MinionConditionData.growth1MinionKeys.Length == 2)
            //     {
            //         if(equiptedMinionKeys.Contains(growth2MinionConditionData.growth1MinionKeys[0]) 
            //         && equiptedMinionKeys.Contains(growth2MinionConditionData.growth1MinionKeys[1]))
            //         {
            //             canPickGrowth2Minions.Add(growth2MinionConditionData.key);
            //         }
            //     }
            // }

            // for (int i = 0; i < equipedSlotPanels.Length; i++)
            // {
            //     equipedSlotPanels[i].UpdatePanel();
            // }

            for (int i = 0; i < presetButtons.Length; i++)
            {
                presetButtons[i].UpdateButton();
            }

            for (int i = 0; i < growth1MinionEntryPanels.Length; i++)
            {
                growth1MinionEntryPanels[i].UpdatePanel();
            }

            // 화면 순서: 보유(등급 낮은 순) → 미보유(등급 낮은 순). 같은 등급은 데이터 순서 유지 (OrderBy는 안정 정렬)
            // 새로 얻은 슬라임도 다음 갱신 때 보유 쪽 등급 자리로 옮겨짐
            var sortedPanels = growth1MinionEntryPanels
                .Where(p => p.minionData != null)
                .OrderByDescending(p => p.userMinion != null && p.userMinion.own)
                .ThenBy(p => p.minionData.grade);
            foreach (Growth1MinionEntryPanel panel in sortedPanels)
            {
                if (panel.transform.parent != ownContainer)
                    panel.transform.SetParent(ownContainer, false);
                panel.transform.SetAsLastSibling();
            }

            // for (int i = 0; i < growth2MinionEntryPanels.Length; i++)
            // {
            //     growth2MinionEntryPanels[i].UpdatePanel();
            // }

            // growth2MinionEntryPanels = growth2MinionEntryPanels
            //     .OrderByDescending(panel => panel.canSpawn)
            //     .ThenBy(panel => panel.selling)
            //     .ToArray();

            // for (int i = 0; i < growth2MinionEntryPanels.Length; i++)
            // {
            //     growth2MinionEntryPanels[i].transform.SetSiblingIndex(i);
            // }
        }
    }
}

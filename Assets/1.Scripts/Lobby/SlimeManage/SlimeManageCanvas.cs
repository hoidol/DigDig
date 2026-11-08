using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lobby
{
    public class SlimeManageCanvas : BaseLobbyCanvas
    {
        public SlimeEquipedSlotPanel[] equipedSlotPanels;
        public Growth1SlimeEntryPanel[] growth1SlimeEntryPanels;
        public Transform ownContainer;
        public Transform unownContainer;
        // public Growth2SlimeEntryPanel[] growth2SlimeEntryPanels;

        public override void Init()
        {
            if (init)
                return;
            init = true;

            // 칸은 SetData에서 켜고 끄므로 꺼져 있는 칸도 포함 (안 그러면 켜진 칸만 잡혀 슬라임이 일부만 보임)
            equipedSlotPanels = GetComponentsInChildren<SlimeEquipedSlotPanel>(true);
            growth1SlimeEntryPanels = GetComponentsInChildren<Growth1SlimeEntryPanel>(true);
            // growth2SlimeEntryPanels = GetComponentsInChildren<Growth2SlimeEntryPanel>();
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

            SlimeData[] slimeDatas = SlimeManager.Instance.allSlimeDatas.OrderBy(e =>
            {
                if (UserDataManager.Instance.userSlimeManager.GetUserSlime(e.key).own)
                    return 0;
                else
                    return 1;
            }).ToArray();

            for (int i = 0; i < growth1SlimeEntryPanels.Length; i++)
            {
                if (i < slimeDatas.Length)
                {
                    growth1SlimeEntryPanels[i].SetData(slimeDatas[i]);
                }
                else
                {
                    growth1SlimeEntryPanels[i].SetData(null);
                }
            }

            // for (int i = 0; i < growth2SlimeEntryPanels.Length; i++)
            // {
            //     if (i < SlimeManager.Instance.growth2SlimeDatas.Length)
            //     {
            //         growth2SlimeEntryPanels[i].SetData(SlimeManager.Instance.growth2SlimeDatas[i]);
            //     }
            //     else
            //     {
            //         growth2SlimeEntryPanels[i].SetData(null);
            //     }
            // }
            UpdateCanvas();
        }

        public override void UpdateCanvas()
        {
            // List<string> equiptedSlimeKeys = new List<string>();
            for (int i = 0; i < UserDataManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes.Length; i++)
            {
                equipedSlotPanels[i].SetData(UserDataManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes[i]);
                // equiptedSlimeKeys.Add(UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes[i].key);
            }

            // List<string> canPickGrowth2Slimes = new List<string>();
            // for(int i =0;i < SlimeManager.Instance.growth2SlimeDatas.Length; i++)
            // {
            //     Growth2SlimeMergeData growth2SlimeConditionData = SlimeManager.Instance.GetGrowth2SlimeMergeData(SlimeManager.Instance.growth2SlimeDatas[i].key); 

            //     if(growth2SlimeConditionData.growth1SlimeKeys.Length == 1)
            //     {
            //         if (equiptedSlimeKeys.Contains(growth2SlimeConditionData.growth1SlimeKeys[0]))
            //         {
            //             canPickGrowth2Slimes.Add(growth2SlimeConditionData.key);
            //         }
            //     }
            //     else if(growth2SlimeConditionData.growth1SlimeKeys.Length == 2)
            //     {
            //         if(equiptedSlimeKeys.Contains(growth2SlimeConditionData.growth1SlimeKeys[0]) 
            //         && equiptedSlimeKeys.Contains(growth2SlimeConditionData.growth1SlimeKeys[1]))
            //         {
            //             canPickGrowth2Slimes.Add(growth2SlimeConditionData.key);
            //         }
            //     }
            // }

            for (int i = 0; i < equipedSlotPanels.Length; i++)
            {
                equipedSlotPanels[i].UpdatePanel();
            }

            for (int i = 0; i < growth1SlimeEntryPanels.Length; i++)
            {
                growth1SlimeEntryPanels[i].UpdatePanel();
            }

            // 보유 여부에 따라 컨테이너를 나눔 (패널 순서 = slimeDatas 정렬 순서 유지)
            for (int i = 0; i < growth1SlimeEntryPanels.Length; i++)
            {
                Growth1SlimeEntryPanel panel = growth1SlimeEntryPanels[i];
                if (panel.slimeData == null)
                    continue;

                Transform container = panel.userSlime.own ? ownContainer : unownContainer;
                if (panel.transform.parent != container)
                    panel.transform.SetParent(container, false);
                panel.transform.SetAsLastSibling();
            }

            // for (int i = 0; i < growth2SlimeEntryPanels.Length; i++)
            // {
            //     growth2SlimeEntryPanels[i].UpdatePanel();
            // }

            // growth2SlimeEntryPanels = growth2SlimeEntryPanels
            //     .OrderByDescending(panel => panel.canSpawn)
            //     .ThenBy(panel => panel.selling)
            //     .ToArray();

            // for (int i = 0; i < growth2SlimeEntryPanels.Length; i++)
            // {
            //     growth2SlimeEntryPanels[i].transform.SetSiblingIndex(i);
            // }
        }
    }
}

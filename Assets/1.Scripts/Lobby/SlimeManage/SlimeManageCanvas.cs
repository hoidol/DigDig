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
        public Growth2SlimeEntryPanel[] growth2SlimeEntryPanels;

        public override void Init()
        {
            if (init)
                return;
            init = true;

            equipedSlotPanels = GetComponentsInChildren<SlimeEquipedSlotPanel>();
            growth1SlimeEntryPanels = GetComponentsInChildren<Growth1SlimeEntryPanel>();
            growth2SlimeEntryPanels = GetComponentsInChildren<Growth2SlimeEntryPanel>();
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

            SlimeData[] growth1SlimeDatas = SlimeManager.Instance.growth1SlimeDatas.OrderBy(e =>
            {
                if (UserManager.Instance.userSlimeManager.GetUserSlime(e.key).own)
                    return 0;
                else
                    return 1;
            }).ToArray();

            for (int i = 0; i < growth1SlimeEntryPanels.Length; i++)
            {
                if (i < growth1SlimeDatas.Length)
                {
                    growth1SlimeEntryPanels[i].SetData(growth1SlimeDatas[i]);
                }
                else
                {
                    growth1SlimeEntryPanels[i].SetData(null);
                }
            }

            for (int i = 0; i < growth2SlimeEntryPanels.Length; i++)
            {
                if (i < SlimeManager.Instance.growth2SlimeDatas.Length)
                {
                    growth2SlimeEntryPanels[i].SetData(SlimeManager.Instance.growth2SlimeDatas[i]);
                }
                else
                {
                    growth2SlimeEntryPanels[i].SetData(null);
                }
            }
            UpdateCanvas();
        }

        public void UpdateCanvas()
        {
            // List<string> equiptedSlimeKeys = new List<string>();
            for (int i = 0; i < UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes.Length; i++)
            {
                equipedSlotPanels[i].SetData(UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes[i]);
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

            for (int i = 0; i < growth2SlimeEntryPanels.Length; i++)
            {
                growth2SlimeEntryPanels[i].UpdatePanel();
            }

            growth2SlimeEntryPanels = growth2SlimeEntryPanels
                .OrderByDescending(panel => panel.canSpawn)
                .ThenBy(panel => panel.selling)
                .ToArray();

            for (int i = 0; i < growth2SlimeEntryPanels.Length; i++)
            {
                growth2SlimeEntryPanels[i].transform.SetSiblingIndex(i);
            }
        }
    }
}

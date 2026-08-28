using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lobby
{
    public class MiniMeManageCanvas : CanvasUI<MiniMeManageCanvas>
    {
        public MiniMeEquipedSlotPanel[] equipedSlotPanels;
        public Growth1MiniMeEntryPanel[]  growth1MiniMeEntryPanels;
        public Growth2MiniMeEntryPanel[] growth2MiniMeEntryPanels;

        public override void OpenCanvas(Action closeCallback = null)
        {
            base.OpenCanvas(closeCallback);
            OpenCanvas();
        }

        public void OpenCanvas()
        {
            for(int i = 0; i < equipedSlotPanels.Length; i++)
            {
                equipedSlotPanels[i].idx = i;
            }


            MiniMeData[] growth1MiniMeDatas = MiniMeManager.Instance.growth1MiniMeDatas.OrderBy(e=>
            {
                if(UserManager.Instance.userMiniMeManager.GetUserMiniMe(e.key).own)
                    return 0;
                else 
                    return 1;
            }).ToArray();

            for(int i = 0; i < growth1MiniMeEntryPanels.Length; i++)
            {
                if (i < growth1MiniMeDatas.Length)
                {
                    growth1MiniMeEntryPanels[i].SetData(growth1MiniMeDatas[i]);
                }
                else
                {
                    growth1MiniMeEntryPanels[i].SetData(null);
                }
            }

            for(int i = 0; i < growth2MiniMeEntryPanels.Length; i++)
            {
                if (i < MiniMeManager.Instance.growth2MiniMeDatas.Length)
                {
                    growth2MiniMeEntryPanels[i].SetData(MiniMeManager.Instance.growth2MiniMeDatas[i]);
                }
                else
                {
                    growth2MiniMeEntryPanels[i].SetData(null);
                }
            }
            UpdateCanvas();
        }

        public void UpdateCanvas()
        {
            // List<string> equiptedMiniMeKeys = new List<string>();
            for(int i = 0; i < UserManager.Instance.userMiniMeManager.userMiniMeData.equiptedMiniMes.Length; i++)
            {
                equipedSlotPanels[i].SetData(UserManager.Instance.userMiniMeManager.userMiniMeData.equiptedMiniMes[i]);
                // equiptedMiniMeKeys.Add(UserManager.Instance.userMiniMeManager.userMiniMeData.equiptedMiniMes[i].key);
            }
           
            // List<string> canPickGrowth2MiniMes = new List<string>();
            // for(int i =0;i < MiniMeManager.Instance.growth2MiniMeDatas.Length; i++)
            // {
            //     Growth2MiniMeMergeData growth2MiniMeConditionData = MiniMeManager.Instance.GetGrowth2MiniMeMergeData(MiniMeManager.Instance.growth2MiniMeDatas[i].key); 
                
            //     if(growth2MiniMeConditionData.growth1MiniMeKeys.Length == 1)
            //     {
            //         if (equiptedMiniMeKeys.Contains(growth2MiniMeConditionData.growth1MiniMeKeys[0]))
            //         {
            //             canPickGrowth2MiniMes.Add(growth2MiniMeConditionData.key);
            //         }
            //     }
            //     else if(growth2MiniMeConditionData.growth1MiniMeKeys.Length == 2)
            //     {
            //         if(equiptedMiniMeKeys.Contains(growth2MiniMeConditionData.growth1MiniMeKeys[0]) 
            //         && equiptedMiniMeKeys.Contains(growth2MiniMeConditionData.growth1MiniMeKeys[1]))
            //         {
            //             canPickGrowth2MiniMes.Add(growth2MiniMeConditionData.key);
            //         }
            //     }
            // }

            for(int i = 0; i < equipedSlotPanels.Length; i++)
            {
                equipedSlotPanels[i].UpdatePanel();
            }

            for(int i = 0; i < growth1MiniMeEntryPanels.Length; i++)
            {
                growth1MiniMeEntryPanels[i].UpdatePanel();
            }
            
            for(int i = 0; i < growth2MiniMeEntryPanels.Length; i++)
            {
                growth2MiniMeEntryPanels[i].UpdatePanel();
            }

            growth2MiniMeEntryPanels = growth2MiniMeEntryPanels
                .OrderByDescending(panel => panel.canSpawn)
                .ThenBy(panel => panel.selling)
                .ToArray();

            for (int i = 0; i < growth2MiniMeEntryPanels.Length; i++)
            {
                growth2MiniMeEntryPanels[i].transform.SetSiblingIndex(i);
            }
        }
    }
}

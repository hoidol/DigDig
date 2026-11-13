using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lobby
{
    public class MinionEquipCanvas : CanvasUI<MinionEquipCanvas>
    {
        public MinionPanel minionPanel;
        public MinionEquipPanel[] equipPanels;
        void Awake()
        {
            for(int i = 0; i < equipPanels.Length; i++)
            {
                equipPanels[i].idx =i;
            }
        }
        UserMinion userMinion;
        UserMinion[] userMinions ;
        public void OpenCanvas(UserMinion userMinion, Action closeCallback = null)
        {
            base.OpenCanvas(closeCallback);
            this.userMinion = userMinion;
            userMinions = UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions;
            for(int i = 0; i < equipPanels.Length; i++)
            {
                equipPanels[i].SetData(userMinions[i].MinionData);
            }
            minionPanel.SetData(userMinion.MinionData);
            OpenCanvas();
        }
        
        public void Selected(int idx)
        {
            UserDataManager.Instance.userMinionManager.EquiptUserMinion(userMinion,idx);
            CloseCanvas();
        }


    }
}

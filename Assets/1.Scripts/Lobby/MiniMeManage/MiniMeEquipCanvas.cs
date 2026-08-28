using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lobby
{
    public class MiniMeEquipCanvas : CanvasUI<MiniMeEquipCanvas>
    {
        public MiniMeEquipPanel[] equipPanels;
        void Awake()
        {
            for(int i = 0; i < equipPanels.Length; i++)
            {
                equipPanels[i].idx =i;
            }
        }
        UserMiniMe userMiniMe;
        UserMiniMe[] userMiniMes ;
        public void OpenCanvas(UserMiniMe userMiniMe, Action closeCallback = null)
        {
            base.OpenCanvas(closeCallback);
            userMiniMes = UserManager.Instance.userMiniMeManager.userMiniMeData.equiptedMiniMes;
            for(int i = 0; i < equipPanels.Length; i++)
            {
                equipPanels[i].SetData(userMiniMes[i].MiniMeData);
            }
            OpenCanvas();
        }
        
        public void Selected(int idx)
        {
            UserManager.Instance.userMiniMeManager.EquiptUserMiniMe(userMiniMe,idx);
            CloseCanvas();
        }


    }
}

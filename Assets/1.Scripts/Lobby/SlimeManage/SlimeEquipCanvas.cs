using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lobby
{
    public class SlimeEquipCanvas : CanvasUI<SlimeEquipCanvas>
    {
        public SlimePanel slimePanel;
        public SlimeEquipPanel[] equipPanels;
        void Awake()
        {
            for(int i = 0; i < equipPanels.Length; i++)
            {
                equipPanels[i].idx =i;
            }
        }
        UserSlime userSlime;
        UserSlime[] userSlimes ;
        public void OpenCanvas(UserSlime userSlime, Action closeCallback = null)
        {
            base.OpenCanvas(closeCallback);
            userSlimes = UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes;
            for(int i = 0; i < equipPanels.Length; i++)
            {
                equipPanels[i].SetData(userSlimes[i].SlimeData);
            }
            slimePanel.SetData(userSlime.SlimeData);
            OpenCanvas();
        }
        
        public void Selected(int idx)
        {
            UserManager.Instance.userSlimeManager.EquiptUserSlime(userSlime,idx);
            CloseCanvas();
        }


    }
}

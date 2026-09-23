using System;
using TMPro;
using UnityEngine;

namespace Lobby
{
    public class UserLevelUpCanvas : CanvasUI<UserLevelUpCanvas> 
    {
        public TMP_Text lvText;
        public RewardPanel[] rewardPanels;

        public override void OpenCanvas(Action closeCallback = null)
        {
            base.OpenCanvas(closeCallback);
            lvText.text = UserDataManager.Instance.userMyInfoManager.userMyInfoData.lv.ToString();
        }
    }    
}

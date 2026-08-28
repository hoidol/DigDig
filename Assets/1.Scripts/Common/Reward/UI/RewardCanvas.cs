using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardCanvas : CanvasUI<RewardCanvas>
{
    List<RewardData> rewardDatas = new List<RewardData>();
    public Image thumImage;
    public TMP_Text valueText;
    public TMP_Text titleText;
    public void OpenCanvas(RewardData rewardData, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        rewardDatas.Add(rewardData);
        ShowReward();
    }
    bool showing;
    RewardData rewardData;
    public void ShowReward()
    {
        if(showing)
            return;
        
        showing =true;

        rewardData = rewardDatas[0];
        rewardDatas.RemoveAt(0);
        
        thumImage.sprite = rewardData.GetThum();
        valueText.text = rewardData.GetValueToString();
        switch (rewardData.rewardType)
        {
            case RewardType.MiniMe:
                titleText.text = TranslateManager.GetText("New Slime");
            break;
        }
    }
    public override void CloseCanvas()
    {
        if(rewardDatas.Count == 0)
            base.CloseCanvas();
        
        showing=false;
        ShowReward();
    }
}
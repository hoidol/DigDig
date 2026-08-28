using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageRewardPanel : MonoBehaviour
{
    public Image thumImage;
    public TMP_Text valueText;
    public GameObject alreadyGotten;
    UserStageReward userStageReward;
    StageRewardData stageRewardData;
    StageData stageData;
    UserStage userStage;
    public void SetRewardData(StageData stageData, UserStage userStage, int idx)
    {
        this.stageData = stageData;
        this.userStage = userStage;
        stageRewardData = stageData.rewardDatas[idx];
        userStageReward = userStage.userStageRewards[idx];
        thumImage.sprite = stageRewardData.GetThum();
        valueText.text = stageRewardData.GetValueToString();
        alreadyGotten.SetActive(userStageReward.gotten);
    }

    public void OnClickedGetReward()
    {
        if(userStageReward.gotten)
            return;
        
        if(userStage.maxPhase < stageRewardData.phase)
        {
            return;
        }
        stageRewardData.Receive();
        UserManager.Instance.userStageManager.ReceiveReward(stageData,stageRewardData.id);
        LobbyCanvas.Instance.UpdateCanvas();
    }
}
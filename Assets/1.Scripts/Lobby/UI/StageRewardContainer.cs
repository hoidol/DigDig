using UnityEngine;

public class StageRewardContainer : MonoBehaviour 
{
    public StageRewardPanel[] stageRewardPanels;
    public void SetStageData(StageData stageData)
    {
        UserStage userStage = UserManager.Instance.userStageManager.GetUserStage(stageData.key);
        
        for(int i = 0; i < stageRewardPanels.Length; i++)
        {
            if (i < stageData.rewardDatas.Length)
            {
                stageRewardPanels[i].SetRewardData(stageData, userStage, i);
            }
        }
    }
}
using UnityEngine;

public class StageRewardContainer : MonoBehaviour
{
    public StageRewardPanel[] stageRewardPanels;
    void Awake()
    {
        stageRewardPanels = GetComponentsInChildren<StageRewardPanel>();
    }
    public void SetStageData(UserStage userStage, StageData stageData)
    {
        // UserStage userStage = UserManager.Instance.userStageManager.GetUserStage(stageData.key);
        Debug.Log($"StageRewardContainer SetStageData {stageData.key} stageRewardPanels.Length {stageRewardPanels.Length}");
        for (int i = 0; i < stageRewardPanels.Length; i++)
        {
            if (i < stageData.rewardDatas.Length)
            {
                stageRewardPanels[i].SetRewardData(stageData, userStage, i);
            }
        }
    }
    public void UpdateContainer()
    {
        Debug.Log($"StageRewardContainer UpdateContainer {stageRewardPanels.Length}");
        for (int i = 0; i < stageRewardPanels.Length; i++)
        {
            stageRewardPanels[i].UpdatePanel();
        }
    }
}
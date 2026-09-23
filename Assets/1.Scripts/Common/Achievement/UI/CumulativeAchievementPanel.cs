using UnityEngine;

public class CumulativeAchievementPanel : AchievementPanel
{
    CumulativeAchievementData cumulativeAchievementData;
    UserCumulativeAchievement userCumulativeAchievement;
    int goal;
    public override void SetAchievementData(AchievementData achievementData)
    {
        base.SetAchievementData(achievementData);
        if(achievementData == null  || achievementData.conditionData.Unlock())
        {
            return;
        }

        gameObject.SetActive(true);
        cumulativeAchievementData = achievementData as CumulativeAchievementData;
        userCumulativeAchievement = cumulativeAchievementData.GetUserAchievement() as UserCumulativeAchievement;
        int goal = cumulativeAchievementData.GetGoal(userCumulativeAchievement.clearCount);
    }

    public override void OnClickedGetReward()
    {
          if(!cumulativeAchievementData.CheckCanClear())
            return;

        AchievementManager.Instance.cumulativeAchievementManager.GetReward(cumulativeAchievementData);
        AchievementCanvas.Instance.UpdateCanvas();
    }

}
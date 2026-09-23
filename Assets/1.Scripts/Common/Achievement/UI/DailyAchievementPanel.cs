using UnityEngine;

public class DailyAchievementPanel : AchievementPanel
{

    DailyAchievementData dailyAchievementData;
    UserDailyAchievement userDailyAchievement;
    public GameObject gottenRewardObj;

    public GameObject redDot;

    public override void SetAchievementData(AchievementData achievementData)
    {

        base.SetAchievementData(achievementData);
        if(achievementData == null || achievementData.conditionData.Unlock())
        {
            return;
        }

        dailyAchievementData = achievementData as DailyAchievementData;
        userDailyAchievement = dailyAchievementData.GetUserAchievement() as UserDailyAchievement;
        
    }

    public override void UpdatePanel()
    {
        base.UpdatePanel();
        if(dailyAchievementData == null)
        {
            return;
        }

        redDot.SetActive(false);
        if (!userDailyAchievement.getReward && dailyAchievementData.CheckCanClear())
        {
            redDot.SetActive(true);
        }
            

        gottenRewardObj.SetActive(userDailyAchievement.getReward);
    }

    public override void OnClickedGetReward()
    {
        if(!dailyAchievementData.CheckCanClear())
            return;

        if(userDailyAchievement.getReward)
            return;

        AchievementManager.Instance.dailyAchievementManager.GetReward(dailyAchievementData);
        AchievementCanvas.Instance.UpdateCanvas();
    }

}
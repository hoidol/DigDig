using UnityEngine;

public class DailyAchievementPanel : AchievementPanel
{

    DailyAchievementData dailyAchievementData;
    UserDailyAchievement userDailyAchievement;
    public GameObject gottenRewardObj;

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

        titleText.text = dailyAchievementData.Title;
        descText.text = dailyAchievementData.Desc();
        gottenRewardObj.SetActive(userDailyAchievement.getReward);
    }

    public override void OnClickedGetReward()
    {
        if(!dailyAchievementData.CheckClear())
            return;

        if(userDailyAchievement.getReward)
            return;

        AchievementManager.Instance.dailyAchievementManager.GetReward(dailyAchievementData);
        AchievementCanvas.Instance.UpdateCanvas();
    }

}
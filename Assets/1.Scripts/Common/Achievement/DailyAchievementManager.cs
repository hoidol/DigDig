using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DailyAchievementManager : SubAchievementManager
{
    public UserDailyAchievementManager userDailyAchievementManager;
    public DailyAchievementData[] dailyAchievementDatas;

    public override void Init()
    {
        userDailyAchievementManager = new UserDailyAchievementManager();
        category = AchievementCategory.Daily;
    }


    public override void Achieve(AchievementType type)
    {
        
    }

    public override void GetReward(AchievementData achievementData)
    {
        achievementData.rewardData.Receive();

        UserDailyAchievement userDailyAchievement = achievementData.GetUserAchievement() as UserDailyAchievement;
        userDailyAchievement.getReward = true;
        userDailyAchievementManager.SaveData();        
    }

    public override int GetCanClearCount()
    {
        int count = 0;
        for(int i = 0; i < dailyAchievementDatas.Length; i++)
        {
            UserDailyAchievement userAchievement = userDailyAchievementManager.GetUserAchievement(dailyAchievementDatas[i].type) as UserDailyAchievement;
            if(!userAchievement.getReward && dailyAchievementDatas[i].CheckCanClear())
            {
                count++;
            }
        }
        return count;
    }
}

public class DailyAchievementData :AchievementData
{
    public int goal;

    public override UserAchievement GetUserAchievement()
    {
        return AchievementManager.Instance.dailyAchievementManager.userDailyAchievementManager.GetUserAchievement(type) as UserDailyAchievement;
    }


    public override bool CheckCanClear()
    {
        UserDailyAchievement achievement = GetUserAchievement() as UserDailyAchievement;
        return achievement.value >=goal;
    }

    public override int GetGoal()
    {
        return goal;
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CumulativeAchievementManager : SubAchievementManager
{
    public UserCumulativeAchievementManager userCumulativeAchievementManager;
    public CumulativeAchievementData[] cumulativeAchievementDatas;
    public override void Init()
    {
        userCumulativeAchievementManager = new UserCumulativeAchievementManager();
    }


    public override void Achieve(AchievementType type)
    {
        UserCumulativeAchievement userCumulativeAchievement = userCumulativeAchievementManager.GetUserAchievement(type) as UserCumulativeAchievement;
        userCumulativeAchievement.value++;
    }
    
    public AchievementData GetAchievementData(AchievementType type)
    {
        for(int i = 0; i < cumulativeAchievementDatas.Length; i++)
        {
            if(cumulativeAchievementDatas[i].type == type)
            {
                return cumulativeAchievementDatas[i];
            }
        }
        return null;
    }

    public override void GetReward(AchievementData achievementData)
    {
        achievementData.rewardData.Receive();

        UserCumulativeAchievement userCumulative = achievementData.GetUserAchievement() as UserCumulativeAchievement;
        int goal = achievementData.GetGoal();
        int remainValue = userCumulative.value - goal;

        userCumulative.clearCount++;
        userCumulative.value = remainValue;
        userCumulativeAchievementManager.SaveData();
    }
}

public class CumulativeAchievementData : AchievementData
{
    public int initGoal;
    public int increaseGoal;
    public int GetGoal(int clearCount)
    {
        return initGoal + increaseGoal * clearCount;
    }
    public override UserAchievement GetUserAchievement()
    {
        return AchievementManager.Instance.cumulativeAchievementManager.userCumulativeAchievementManager.GetUserAchievement(type) as UserCumulativeAchievement;
    }

    public override bool CheckClear()
    {
        UserCumulativeAchievement achievement = GetUserAchievement() as UserCumulativeAchievement;
        return achievement.value >= GetGoal(achievement.clearCount);
    }

    public override int GetGoal()
    {
        UserCumulativeAchievement userAchievement =  GetUserAchievement() as UserCumulativeAchievement;
        return GetGoal(userAchievement.clearCount);
    }
}
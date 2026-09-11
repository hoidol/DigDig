using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserCumulativeAchievementManager :UserSubAchievementManager
{

    [field: SerializeField]
    public UserCumulativeAchievementData  userCumulativeAchievementData
    {
        get; private set;
    }
    public UserCumulativeAchievementManager()
    {
        UserDataFileName = "UserCumulativeAchievement";
        LoadData();
    }
    public override void LoadData()
    {
        userCumulativeAchievementData = SaveManager.LoadData<UserCumulativeAchievementData>(UserDataFileName);
        
        if (userCumulativeAchievementData == null)
        {
            userCumulativeAchievementData = new UserCumulativeAchievementData();
        }
        userAchievementData = userCumulativeAchievementData;

        SaveData();
    }




    public UserAchievement GetUserAchievement(AchievementType type)
    {
        for(int i = 0; i < userCumulativeAchievementData.userCumulativeAchievements.Count; i++)
        {
            if(userCumulativeAchievementData.userCumulativeAchievements[i].type == type)
            {
                return userCumulativeAchievementData.userCumulativeAchievements[i];
            }
        }
        UserCumulativeAchievement achievement = new UserCumulativeAchievement();
        achievement.type = type;
        userCumulativeAchievementData.userCumulativeAchievements.Add(achievement);
        SaveData();
        return achievement;
    }
}



[System.Serializable]
public class UserCumulativeAchievementData : UserAchievementData
{
    //장비 보유 상태
    public List<UserCumulativeAchievement> userCumulativeAchievements = new List<UserCumulativeAchievement>(); 
}
public class UserAchievement
{
    public AchievementType type;
    public int value;
}

[System.Serializable]
public class UserCumulativeAchievement : UserAchievement
{
    
    public int clearCount;
    public CumulativeAchievementData achievementData
    {
        get
        {
            return AchievementManager.Instance.cumulativeAchievementManager.GetAchievementData(type) as CumulativeAchievementData;
        }
    }
    
}
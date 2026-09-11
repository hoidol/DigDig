using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserDailyAchievementManager : UserSubAchievementManager
{

    [field: SerializeField]
    public UserDailyAchievementData  userDailyAchievementData
    {
        get; private set;
    }
    public UserDailyAchievementManager()
    {   
        UserDataFileName = "UserDailyAchievement";
        LoadData();
    }
    
    public override void LoadData()
    {
        userDailyAchievementData = SaveManager.LoadData<UserDailyAchievementData>(UserDataFileName);
        
        
        if (userDailyAchievementData == null)
        {
            userDailyAchievementData = new UserDailyAchievementData();
        }
        userAchievementData = userDailyAchievementData;

        SaveData();
    }





    public UserAchievement GetUserAchievement(AchievementType type)
    {
        for(int i = 0; i < userDailyAchievementData.userDailyAchievements.Count; i++)
        {
            if(userDailyAchievementData.userDailyAchievements[i].type == type)
            {
                return userDailyAchievementData.userDailyAchievements[i];
            }
        }

        UserDailyAchievement achievement = new UserDailyAchievement();
        achievement.type = type;
        userDailyAchievementData.userDailyAchievements.Add(achievement);
        SaveData();
        return achievement;
    }
}



[System.Serializable]
public class UserDailyAchievementData : UserAchievementData
{
    //장비 보유 상태
    public List<UserDailyAchievement> userDailyAchievements = new List<UserDailyAchievement>(); 
}

[System.Serializable]
public class UserDailyAchievement : UserAchievement
{
    public bool getReward;
}
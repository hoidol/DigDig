using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class UserDailyAchievementManager : UserSubAchievementManager
{

    [field: SerializeField]
    public UserDailyAchievementData userDailyAchievementData
    {
        get; private set;
    }
    bool isLoaded;
    void Awake()
    {
        EnsureLoaded();
    }

    // 부모 매니저 Awake가 먼저 돌 수 있어서, 누가 먼저 부르든 한 번만 로드
    public void EnsureLoaded()
    {
        if (isLoaded)
            return;
        isLoaded = true;
        UserDataFileName = "UserDailyAchievement";
        LoadData();
    }

    public override void LoadData()
    {
        UserDailyAchievementData uDailyAchievementData = SaveManager.LoadData<UserDailyAchievementData>(UserDataFileName);

        Debug.Log($"UserDailyAchievementManager LoadData() UserDataFileName {UserDataFileName}");
        if (uDailyAchievementData == null)
        {
            uDailyAchievementData = new UserDailyAchievementData();
        }
        userAchievementData = uDailyAchievementData;
        userDailyAchievementData = uDailyAchievementData;

        SaveData();
    }





    public UserDailyAchievement GetUserAchievement(AchievementType type)
    {
        for (int i = 0; i < userDailyAchievementData.userDailyAchievements.Count; i++)
        {
            if (userDailyAchievementData.userDailyAchievements[i].type == type)
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
    public string date; // 이 기록의 날짜 (DailyCheck.Today). 오늘이 아니면 초기화
    public List<UserDailyAchievement> userDailyAchievements = new List<UserDailyAchievement>();
}

[System.Serializable]
public class UserDailyAchievement : UserAchievement
{
    public bool getReward;
}
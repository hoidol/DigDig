using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
[System.Serializable]
public class UserCumulativeAchievementManager : UserSubAchievementManager
{

    [field: SerializeField]
    public UserCumulativeAchievementData userCumulativeAchievementData
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
        UserDataFileName = "UserCumulativeAchievement";
        LoadData();
    }
    public override void LoadData()
    {
        Debug.Log($"UserCumulativeAchievementManager LoadData() UserDataFileName {UserDataFileName}");
        UserCumulativeAchievementData uCumulativeAchievementData = SaveManager.LoadData<UserCumulativeAchievementData>(UserDataFileName);

        if (uCumulativeAchievementData == null)
        {
            uCumulativeAchievementData = new UserCumulativeAchievementData();
        }
        userAchievementData = uCumulativeAchievementData;
        userCumulativeAchievementData = uCumulativeAchievementData;

        SaveData();
    }




    public UserCumulativeAchievement GetUserAchievement(AchievementType type)
    {
        for (int i = 0; i < userCumulativeAchievementData.userCumulativeAchievements.Count; i++)
        {
            if (userCumulativeAchievementData.userCumulativeAchievements[i].type == type)
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
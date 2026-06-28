using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[System.Serializable]
public class UserStageManager : UserBaseManager
{
    public const string UserDataFileName = "UserStageData";
    [field: SerializeField]
    public UserStageData userStageData
    {
        get; private set;
    }

    public UserStage GetCurrentStage()
    {
        for (int i = 0; i < userStageData.userStages.Count; i++)
        {
            if (userStageData.userStages[i].clearCount > 0)
                continue;
            return userStageData.userStages[i];
        }

        return userStageData.userStages[userStageData.userStages.Count - 1]; //마지막거 보내주기
    }

    public UserStage GetUserStage(int order)
    {
        return userStageData.userStages.Where(e => StageData.GetStageData(e.key).order == order).FirstOrDefault();
    }

    public override void LoadData()
    {
        userStageData = SaveManager.LoadData<UserStageData>(UserDataFileName);
        if (userStageData == null)
        {
            userStageData = new UserStageData();
            UserStage userStage = new UserStage();
            userStage.key = GameSetting.FIRST_STAGE_KEY;

            userStageData.userStages.Add(userStage);
            SaveData();
        }
    }

    public void ClearStage(string key)
    {
        UserStage userStage = GetUserStage(key);
        userStage.clearCount++;
        SaveData();
    }

    public void StartStage(string key)
    {
        UserStage userStage = GetUserStage(key);
        userStage.tryCount++;
        SaveData();
    }

    public UserStage GetUserStage(string key)
    {
        UserStage userStage = userStageData.userStages.Where(e => e.key == key).FirstOrDefault();
        if (userStage == null)
        {
            userStage = new UserStage();
            userStage.key = key;
            userStageData.userStages.Add(userStage);
            userStageData.userStages = userStageData.userStages.OrderBy(e => StageData.GetStageData(key).order).ToList();
            SaveData();
        }
        return userStage;
    }

    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userStageData);
    }
}

[System.Serializable]
public class UserStageData
{
    public List<UserStage> userStages = new List<UserStage>();
}
[System.Serializable]
public class UserStage
{
    public string key;
    public int tryCount;
    public int clearCount;
}
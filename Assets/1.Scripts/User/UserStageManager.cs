using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.SceneManagement;
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

    public void EndStage(string key, bool clear, int maxPhase)
    {
        
        UserStage userStage = GetUserStage(key);

        if (clear)
        {
            userStage.clearCount++;    
            userStage.maxPhase = maxPhase;
        }
        else
        {
            if(maxPhase > userStage.maxPhase)
                userStage.maxPhase = maxPhase;
        }
        
        SaveData();
    }

    public void TryStage(string key)
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

    public string GetMaxStage()
    {
        int order = 0;
        for(int i = 0; i < userStageData.userStages.Count; i++)
        {
            if (userStageData.userStages[i].clearCount > 0)
            {
                order++;
            }
        }
        StageData stageData = StageManager.Instance.GetStageData(order);
        if(stageData == null)
        {
            stageData = StageManager.Instance.GetStageData(StageManager.Instance.stageDatas.Length-1);
        }

        return stageData.key;
    }

    public void ReceiveReward(StageData stageData , string id)
    {
        UserStage userStage = GetUserStage(stageData.key);
        userStage.GetUserStageReward(id).gotten =true;
        SaveData();
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
    public int maxPhase;
    public List<UserStageReward> userStageRewards = new List<UserStageReward>();
    public UserStageReward GetUserStageReward(string id)
    {
        return userStageRewards.Where(e => e.id ==id).FirstOrDefault();
    }


}
[System.Serializable]
public class UserStageReward : UserReward
{
    public int clearCount;
}
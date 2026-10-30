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

    // 현재 선택한 난이도의 스테이지 (난이도마다 스테이지 하나)
    public UserStage GetCurrentStage()
    {
        return GetUserStage(UserDataManager.difficultyType);
    }

    // 난이도마다 스테이지 하나, key = 난이도 이름
    public UserStage GetUserStage(DifficultyType difficultyType)
    {
        return GetUserStage(difficultyType.ToString());
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
            RequestSave();
        }
        MigrateStageKeys();
    }

    // 이전 저장 데이터: 난이도별 여러 스테이지("Normal_0" ~ "Normal_4") → 난이도당 하나("Normal").
    // "{난이도}_0" 기록은 새 key로 옮기고 나머지 스테이지 기록은 제거
    void MigrateStageKeys()
    {
        bool changed = false;
        for (int i = userStageData.userStages.Count - 1; i >= 0; i--)
        {
            UserStage userStage = userStageData.userStages[i];
            if (System.Enum.TryParse(userStage.key, out DifficultyType _))
                continue;

            string[] split = userStage.key.Split('_');
            bool isFirst = split.Length == 2 && split[1] == "0" && System.Enum.TryParse(split[0], out DifficultyType _);
            if (isFirst && !userStageData.userStages.Any(e => e.key == split[0]))
                userStage.key = split[0];
            else
                userStageData.userStages.RemoveAt(i);
            changed = true;
        }
        if (changed)
            RequestSave();
    }

    public void EndStage(string key,  int maxWave)
    {

        UserStage userStage = GetUserStage(key);

        if (maxWave > userStage.maxWave)
            userStage.maxWave = maxWave;

        RequestSave();
    }

    public void TryStage(string key)
    {
        UserStage userStage = GetUserStage(key);
        userStage.tryCount++;
        RequestSave();
    }

    public UserStage GetUserStage(string key)
    {
        UserStage userStage = userStageData.userStages.Where(e => e.key == key).FirstOrDefault();
        if (userStage == null)
        {
            userStage = new UserStage();
            userStage.key = key;
            userStageData.userStages.Add(userStage);
            RequestSave();
        }
        return userStage;
    }

    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userStageData);
    }

    public void ReceiveReward(StageData stageData, string id)
    {
        UserStage userStage = GetUserStage(stageData.key);
        userStage.GetUserStageReward(id).gotten = true;
        RequestSave();
    }
}

[System.Serializable]
public class UserStageData
{

    // public DifficultyType difficultyType;// 유저가 현재 선택한 난이도
    public List<UserStage> userStages = new List<UserStage>();

}
[System.Serializable]
public class UserStage
{
    public string key;
    public int tryCount;
    public int maxWave;
    public List<UserStageReward> userStageRewards = new List<UserStageReward>();
    public UserStageReward GetUserStageReward(string id)
    {
        return userStageRewards.Where(e => e.id == id).FirstOrDefault();
    }


}
[System.Serializable]
public class UserStageReward : UserReward
{
    public int clearCount;
}
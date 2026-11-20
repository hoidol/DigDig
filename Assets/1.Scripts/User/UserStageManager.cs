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

    // 현재 선택한 스테이지·난이도 (UserDataManager.stageOrder / difficultyType)
    public UserStage GetCurrentStage()
    {
        return GetUserStage(UserDataManager.stageOrder, UserDataManager.difficultyType);
    }

    // 스테이지 번호 × 난이도마다 기록 하나, key = "{난이도}_{order}" (StageData.MakeKey)
    public UserStage GetUserStage(int order, DifficultyType difficultyType)
    {
        return GetUserStage(StageData.MakeKey(order, difficultyType));
    }

    // 없으면 null (GetUserStage는 없으면 만들어서 저장함)
    public UserStage FindUserStage(int order, DifficultyType difficultyType)
    {
        string key = StageData.MakeKey(order, difficultyType);
        return userStageData.userStages.FirstOrDefault(e => e.key == key);
    }

    // 그 난이도의 모든 스테이지 중 최고 기록 (표시 번호 = maxWave + 1). 기록 없으면 0
    public int GetBestWave(DifficultyType difficultyType)
    {
        int best = 0;
        foreach (UserStage userStage in userStageData.userStages)
        {
            if (TryParseKey(userStage.key, out _, out DifficultyType d) && d == difficultyType && userStage.tryCount > 0)
                best = Mathf.Max(best, userStage.maxWave + 1);
        }
        return best;
    }

    // "Hard_1" → (1, Hard)
    public static bool TryParseKey(string key, out int order, out DifficultyType difficultyType)
    {
        order = 0;
        difficultyType = DifficultyType.Normal;
        if (string.IsNullOrEmpty(key))
            return false;
        string[] split = key.Split('_');
        return split.Length == 2 && System.Enum.TryParse(split[0], out difficultyType) && int.TryParse(split[1], out order);
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

    // 이전 저장 데이터: 난이도당 하나("Normal", 2026-10-30~11-20) → 스테이지 key "{난이도}_{order}"의 첫 스테이지("Normal_0")
    void MigrateStageKeys()
    {
        bool changed = false;
        for (int i = userStageData.userStages.Count - 1; i >= 0; i--)
        {
            UserStage userStage = userStageData.userStages[i];
            if (TryParseKey(userStage.key, out _, out _))
                continue;

            string newKey = System.Enum.TryParse(userStage.key, out DifficultyType difficulty)
                ? StageData.MakeKey(StageData.FIRST_STAGE_ORDER, difficulty) : null;
            if (newKey != null && !userStageData.userStages.Any(e => e.key == newKey))
                userStage.key = newKey;
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

    // 마지막 wave(GameSetting.LAST_WAVE)까지 깬 난이도. 다음 난이도 해금 기준
    public void ClearStage(string key)
    {
        UserStage userStage = GetUserStage(key);
        if (userStage.isClear)
            return;
        userStage.isClear = true;
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
    public bool isClear; // 마지막 wave까지 깸 (2026-11-19)
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
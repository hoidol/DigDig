using System.Collections.Generic;
using UnityEngine;

// 일일 업적: 목표(Goal) 달성 → 보상 1번. 날짜가 바뀌면 진행도·수령 여부 모두 초기화
public class DailyAchievementManager : SubAchievementManager
{
    public const string CSV_PATH = "Json/DailyAchievement";
    public UserDailyAchievementManager userDailyAchievementManager;
    public DailyAchievementData[] dailyAchievementDatas;

    public override void Init()
    {
        userDailyAchievementManager = GetComponentInChildren<UserDailyAchievementManager>();
        userDailyAchievementManager.EnsureLoaded();
        category = AchievementCategory.Daily;

        TextAsset csv = Resources.Load<TextAsset>(CSV_PATH);
        if (csv == null)
        {
            Debug.LogWarning("[DailyAchievementManager] CSV 파일 없음: Resources/Json/DailyAchievement.csv");
            dailyAchievementDatas = new DailyAchievementData[0];
            return;
        }
        dailyAchievementDatas = ParseCsv(csv.text).ToArray();
    }

    // Type / Goal / RewardType / RewardValue
    public static List<DailyAchievementData> ParseCsv(string text)
    {
        List<DailyAchievementData> list = new List<DailyAchievementData>();
        foreach (AchievementCsv.Row row in AchievementCsv.Parse(text))
        {
            if (!AchievementCsv.TryGetTypes(row, "DailyAchievement.csv", out AchievementType type, out RewardData rewardData))
                continue;
            list.Add(new DailyAchievementData { type = type, goal = row.GetInt("Goal"), rewardData = rewardData });
        }
        return list;
    }

    // 저장은 AchievementManager가 프레임 끝에 한 번
    public override void Achieve(AchievementType type, int count)
    {
        userDailyAchievementManager.GetUserAchievement(type).value += count;
    }

    // 날짜가 바뀌었으면 초기화하고 true
    public bool ResetIfNewDay()
    {
        UserDailyAchievementData data = userDailyAchievementManager.userDailyAchievementData;
        if (DailyCheck.IsToday(data.date))
            return false;
        data.userDailyAchievements.Clear();
        data.date = DailyCheck.Today;
        userDailyAchievementManager.SaveData();
        return true;
    }

    public override void GetReward(AchievementData achievementData)
    {
        UserDailyAchievement userDailyAchievement = achievementData.GetUserAchievement() as UserDailyAchievement;
        if (userDailyAchievement.getReward || !achievementData.CheckCanClear())
            return;

        achievementData.rewardData.Receive();
        userDailyAchievement.getReward = true;
        userDailyAchievementManager.SaveData();
    }

    public override int GetCanClearCount()
    {
        int count = 0;
        for (int i = 0; i < dailyAchievementDatas.Length; i++)
        {
            UserDailyAchievement userAchievement = userDailyAchievementManager.GetUserAchievement(dailyAchievementDatas[i].type);
            if (!userAchievement.getReward && dailyAchievementDatas[i].CheckCanClear())
            {
                count++;
            }
        }
        return count;
    }
}

[System.Serializable]
public class DailyAchievementData : AchievementData
{
    public int goal;

    public override UserAchievement GetUserAchievement()
    {
        return AchievementManager.Instance.dailyAchievementManager.userDailyAchievementManager.GetUserAchievement(type);
    }

    public override bool CheckCanClear()
    {
        return GetUserAchievement().value >= goal;
    }

    public override int GetGoal()
    {
        return goal;
    }

    protected override int GetDisplayValue() => Mathf.Min(GetUserAchievement().value, goal);
}

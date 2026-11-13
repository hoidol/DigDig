using System.Collections.Generic;
using UnityEngine;

// 누적 업적: 목표 = InitGoal + IncreaseGoal × 클리어 횟수. 보상을 받으면 목표만큼 빼고(초과분은 이월) 다음 목표로
public class CumulativeAchievementManager : SubAchievementManager
{
    public const string CSV_PATH = "Json/CumulativeAchievement";
    public UserCumulativeAchievementManager userCumulativeAchievementManager;
    public CumulativeAchievementData[] cumulativeAchievementDatas;
    public override void Init()
    {
        userCumulativeAchievementManager = GetComponentInChildren<UserCumulativeAchievementManager>();
        userCumulativeAchievementManager.EnsureLoaded();
        category = AchievementCategory.Cumulative;

        TextAsset csv = Resources.Load<TextAsset>(CSV_PATH);
        if (csv == null)
        {
            Debug.LogWarning("[CumulativeAchievementManager] CSV 파일 없음: Resources/Json/CumulativeAchievement.csv");
            cumulativeAchievementDatas = new CumulativeAchievementData[0];
            return;
        }
        cumulativeAchievementDatas = ParseCsv(csv.text).ToArray();
    }

    // Type / InitGoal / IncreaseGoal / RewardType / RewardValue
    public static List<CumulativeAchievementData> ParseCsv(string text)
    {
        List<CumulativeAchievementData> list = new List<CumulativeAchievementData>();
        foreach (AchievementCsv.Row row in AchievementCsv.Parse(text))
        {
            if (!AchievementCsv.TryGetTypes(row, "CumulativeAchievement.csv", out AchievementType type, out RewardData rewardData))
                continue;
            list.Add(new CumulativeAchievementData
            {
                type = type,
                initGoal = row.GetInt("InitGoal"),
                increaseGoal = row.GetInt("IncreaseGoal"),
                rewardData = rewardData
            });
        }
        return list;
    }

    // 저장은 AchievementManager가 프레임 끝에 한 번
    public override void Achieve(AchievementType type, int count)
    {
        userCumulativeAchievementManager.GetUserAchievement(type).value += count;
    }

    public AchievementData GetAchievementData(AchievementType type)
    {
        for (int i = 0; i < cumulativeAchievementDatas.Length; i++)
        {
            if (cumulativeAchievementDatas[i].type == type)
            {
                return cumulativeAchievementDatas[i];
            }
        }
        return null;
    }

    public override void GetReward(AchievementData achievementData)
    {
        if (!achievementData.CheckCanClear())
            return;

        achievementData.rewardData.Receive();

        UserCumulativeAchievement userCumulative = achievementData.GetUserAchievement() as UserCumulativeAchievement;
        int goal = achievementData.GetGoal();
        userCumulative.value -= goal;
        userCumulative.clearCount++;
        userCumulativeAchievementManager.SaveData();
    }


    public override int GetCanClearCount()
    {
        int count = 0;
        for (int i = 0; i < cumulativeAchievementDatas.Length; i++)
        {
            if (cumulativeAchievementDatas[i].CheckCanClear())
            {
                count++;
            }
        }
        return count;
    }
}
[System.Serializable]
public class CumulativeAchievementData : AchievementData
{
    public int initGoal;
    public int increaseGoal;
    public int GetGoal(int clearCount) => GetGoal(initGoal, increaseGoal, clearCount);

    public static int GetGoal(int initGoal, int increaseGoal, int clearCount) => initGoal + increaseGoal * clearCount;

    public override UserAchievement GetUserAchievement()
    {
        return AchievementManager.Instance.cumulativeAchievementManager.userCumulativeAchievementManager.GetUserAchievement(type);
    }

    public override bool CheckCanClear()
    {
        UserCumulativeAchievement achievement = GetUserAchievement() as UserCumulativeAchievement;
        return achievement.value >= GetGoal(achievement.clearCount);
    }

    public override int GetGoal()
    {
        UserCumulativeAchievement userAchievement = GetUserAchievement() as UserCumulativeAchievement;
        return GetGoal(userAchievement.clearCount);
    }
}

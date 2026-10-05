using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DailyAchievementManager : SubAchievementManager
{
    public UserDailyAchievementManager userDailyAchievementManager;
    public DailyAchievementData[] dailyAchievementDatas;

    public override void Init()
    {
        userDailyAchievementManager = GetComponentInChildren<UserDailyAchievementManager>();
        category = AchievementCategory.Daily;
        SetDailyAchievementData();
    }
    void SetDailyAchievementData()
    {
        List<DailyAchievementData> list = new List<DailyAchievementData>();
        TextAsset csv = Resources.Load<TextAsset>("Json/DailyAchievement");
        if (csv == null)
        {
            Debug.LogWarning("[DailyAchievementManager] CSV 파일 없음: Resources/Json/DailyAchievement.csv");
            dailyAchievementDatas = list.ToArray();
            return;
        }

        string[] lines = csv.text.Split('\n');
        string[] headers = lines[0].Trim().Split('\t');
        int iType = System.Array.IndexOf(headers, "type");
        int iGoal = System.Array.IndexOf(headers, "goal");
        int iRewardType = System.Array.IndexOf(headers, "rewardType");
        int iRewardValue = System.Array.IndexOf(headers, "rewardValue");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Trim().Split('\t');

            if (!System.Enum.TryParse(cols[iType].Trim(), out AchievementType type)) continue;
            if (!System.Enum.TryParse(cols[iRewardType].Trim(), out RewardType rewardType)) continue;

            list.Add(new DailyAchievementData
            {
                type = type,
                goal = int.Parse(cols[iGoal].Trim()),
                rewardData = new RewardData { rewardType = rewardType, value = cols[iRewardValue].Trim() }
            });
        }
        dailyAchievementDatas = list.ToArray();
    }

    public override void Achieve(AchievementType type, int count)
    {
        UserDailyAchievement userDailyAchievement = userDailyAchievementManager.GetUserAchievement(type) as UserDailyAchievement;
        userDailyAchievement.value += count;
        userDailyAchievementManager.SaveData();
    }

    public override void GetReward(AchievementData achievementData)
    {
        achievementData.rewardData.Receive();

        UserDailyAchievement userDailyAchievement = achievementData.GetUserAchievement() as UserDailyAchievement;
        userDailyAchievement.getReward = true;
        userDailyAchievementManager.SaveData();
    }

    public override int GetCanClearCount()
    {
        int count = 0;
        for (int i = 0; i < dailyAchievementDatas.Length; i++)
        {
            UserDailyAchievement userAchievement = userDailyAchievementManager.GetUserAchievement(dailyAchievementDatas[i].type) as UserDailyAchievement;
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
        return AchievementManager.Instance.dailyAchievementManager.userDailyAchievementManager.GetUserAchievement(type) as UserDailyAchievement;
    }


    public override bool CheckCanClear()
    {
        UserDailyAchievement achievement = GetUserAchievement() as UserDailyAchievement;
        return achievement.value >= goal;
    }

    public override int GetGoal()
    {
        return goal;
    }
}
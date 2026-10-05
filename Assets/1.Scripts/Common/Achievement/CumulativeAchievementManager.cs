using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CumulativeAchievementManager : SubAchievementManager
{
    public UserCumulativeAchievementManager userCumulativeAchievementManager;
    public CumulativeAchievementData[] cumulativeAchievementDatas;
    public override void Init()
    {
        userCumulativeAchievementManager = GetComponentInChildren<UserCumulativeAchievementManager>();
        category = AchievementCategory.Cumulative;
        SetCumulativeAchievementData();
    }
    void SetCumulativeAchievementData()
    {
        List<CumulativeAchievementData> list = new List<CumulativeAchievementData>();
        TextAsset csv = Resources.Load<TextAsset>("Json/CumulativeAchievement");
        if (csv == null)
        {
            Debug.LogWarning("[CumulativeAchievementManager] CSV 파일 없음: Resources/Json/CumulativeAchievement.csv");
            cumulativeAchievementDatas = list.ToArray();
            return;
        }

        string[] lines = csv.text.Split('\n');
        string[] headers = lines[0].Trim().Split('\t');
        int iType = System.Array.IndexOf(headers, "type");
        int iInitGoal = System.Array.IndexOf(headers, "initGoal");
        int iIncreaseGoal = System.Array.IndexOf(headers, "increaseGoal");
        int iRewardType = System.Array.IndexOf(headers, "rewardType");
        int iRewardValue = System.Array.IndexOf(headers, "rewardValue");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Trim().Split('\t');

            if (!System.Enum.TryParse(cols[iType].Trim(), out AchievementType type)) continue;
            if (!System.Enum.TryParse(cols[iRewardType].Trim(), out RewardType rewardType)) continue;

            list.Add(new CumulativeAchievementData
            {
                type = type,
                initGoal = int.Parse(cols[iInitGoal].Trim()),
                increaseGoal = int.Parse(cols[iIncreaseGoal].Trim()),
                rewardData = new RewardData { rewardType = rewardType, value = cols[iRewardValue].Trim() }
            });
        }
        cumulativeAchievementDatas = list.ToArray();
    }


    public override void Achieve(AchievementType type, int count)
    {
        UserCumulativeAchievement userCumulativeAchievement = userCumulativeAchievementManager.GetUserAchievement(type) as UserCumulativeAchievement;
        userCumulativeAchievement.value += count;
        userCumulativeAchievementManager.SaveData();
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
        achievementData.rewardData.Receive();

        UserCumulativeAchievement userCumulative = achievementData.GetUserAchievement() as UserCumulativeAchievement;
        int goal = achievementData.GetGoal();
        int remainValue = userCumulative.value - goal;

        userCumulative.clearCount++;
        userCumulative.value = remainValue;
        userCumulativeAchievementManager.SaveData();
    }


    public override int GetCanClearCount()
    {
        int count = 0;
        for (int i = 0; i < cumulativeAchievementDatas.Length; i++)
        {
            UserCumulativeAchievement userAchievement = userCumulativeAchievementManager.GetUserAchievement(cumulativeAchievementDatas[i].type) as UserCumulativeAchievement;
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
    public int GetGoal(int clearCount)
    {
        return initGoal + increaseGoal * clearCount;
    }
    public override UserAchievement GetUserAchievement()
    {
        return AchievementManager.Instance.cumulativeAchievementManager.userCumulativeAchievementManager.GetUserAchievement(type) as UserCumulativeAchievement;
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
public class DailyAchievementContainer : AchievementContainer
{
    protected override AchievementData[] GetAchievementDatas() => AchievementManager.Instance.dailyAchievementManager.dailyAchievementDatas;
}

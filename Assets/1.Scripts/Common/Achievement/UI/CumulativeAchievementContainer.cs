public class CumulativeAchievementContainer : AchievementContainer
{
    protected override AchievementData[] GetAchievementDatas() => AchievementManager.Instance.cumulativeAchievementManager.cumulativeAchievementDatas;
}

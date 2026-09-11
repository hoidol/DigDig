using UnityEngine;

public class DailyAchievementContainer : AchievementContainer
{
      public DailyAchievementPanel[] dailyAchievementPanels;
      public override void OpenContainer()
      {
        for(int i = 0; i < AchievementManager.Instance.dailyAchievementManager.dailyAchievementDatas.Length; i++)
        {
            if(i < AchievementManager.Instance.dailyAchievementManager.dailyAchievementDatas.Length)
            {
                achievementPanels[i].SetAchievementData(AchievementManager.Instance.dailyAchievementManager.dailyAchievementDatas[i]);
            }
            else
            {
                achievementPanels[i].SetAchievementData(null);
            }
            
        }
      }

}
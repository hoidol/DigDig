using UnityEngine;

public class CumulativeAchievementContainer : AchievementContainer
{    

    public override void OpenContainer()
    {
        for(int i = 0; i < achievementPanels.Length; i++)
        {
            achievementPanels[i].gameObject.SetActive(false);
        }
        for(int i = 0; i < AchievementManager.Instance.cumulativeAchievementManager.cumulativeAchievementDatas.Length; i++)
        {
            if(i < AchievementManager.Instance.cumulativeAchievementManager.cumulativeAchievementDatas.Length)
            {
                achievementPanels[i].SetAchievementData(AchievementManager.Instance.cumulativeAchievementManager.cumulativeAchievementDatas[i]);
            }
            else
            {
                achievementPanels[i].SetAchievementData(null);
            }
            
        }
    }

}
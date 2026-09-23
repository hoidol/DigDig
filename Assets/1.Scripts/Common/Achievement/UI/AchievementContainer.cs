using UnityEngine;

public abstract class AchievementContainer : MonoBehaviour 
{
    public AchievementPanel[] achievementPanels;
    public virtual void OpenContainer()
    {
        achievementPanels = GetComponentsInChildren<AchievementPanel>();
    }
    public virtual void UpdateContainer()
    {
        for(int i = 0; i < achievementPanels.Length; i++)
        {
            achievementPanels[i].UpdatePanel();
        }
    }


}
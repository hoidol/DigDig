using UnityEngine;

public abstract class AchievementContainer : MonoBehaviour 
{
    public AchievementPanel[] achievementPanels;
    public abstract void OpenContainer();
    public virtual void UpdateContainer()
    {
        for(int i = 0; i < achievementPanels.Length; i++)
        {
            achievementPanels[i].UpdatePanel();
        }
    }


}
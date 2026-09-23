using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class SubAchievementManager
{
    public AchievementCategory category;
    public abstract void Init();   
    public virtual void Achieve(AchievementType type)
    {
        
    }
    public abstract void GetReward(AchievementData achievementData);
    public abstract int GetCanClearCount();
}


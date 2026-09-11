using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AchievementManager : MonoSingleton<AchievementManager>
{
    public DailyAchievementManager dailyAchievementManager;
    public CumulativeAchievementManager cumulativeAchievementManager;
    public List<SubAchievementManager> subAchievementManagers;
    void Awake()
    {
        dailyAchievementManager = new DailyAchievementManager();
        cumulativeAchievementManager = new CumulativeAchievementManager();
        
        subAchievementManagers.Add(dailyAchievementManager);
        subAchievementManagers.Add(cumulativeAchievementManager);

        dailyAchievementManager.Init();
        cumulativeAchievementManager.Init();

        SceneManager.sceneLoaded += OnLoadedScene;
        SceneManager.sceneUnloaded += OnUnloadedScene;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnLoadedScene;
        SceneManager.sceneUnloaded -= OnUnloadedScene;
    }

    void OnLoadedScene(Scene scene, LoadSceneMode mode)
    {
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }


    void OnUnloadedScene(Scene scene)
    {    
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    void OnDestroyedStoneEvent(DestroyedStoneEvent e)
    {
        Achieve(AchievementType.DestroyStone);
    }

    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        Achieve(AchievementType.KillEnemy);
    }

    public void Achieve(AchievementType achievementType)
    {
        for(int i = 0; i < subAchievementManagers.Count; i++)
        {
            subAchievementManagers[i].Achieve(achievementType);
        }
    }
}

public enum AchievementType
{
    DestroyStone,
    KillEnemy,
    ClearStage,
    Login,
    LevelUpSlime
}

public abstract class AchievementData
{
    public AchievementType type;
    public ConditionData conditionData;
    public RewardData rewardData;
    public string Title => $"{type}";
    public string Desc() => $"{GetUserAchievement().value}/{GetGoal()}";

    public abstract UserAchievement GetUserAchievement();
    public abstract bool CheckClear();
    public abstract int GetGoal();
}

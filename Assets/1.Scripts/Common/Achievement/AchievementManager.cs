using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AchievementManager : MonoSingleton<AchievementManager>
{
    public DailyAchievementManager dailyAchievementManager;
    public CumulativeAchievementManager cumulativeAchievementManager;
    public List<SubAchievementManager> subAchievementManagers = new List<SubAchievementManager>();
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
    public SubAchievementManager GetSubAchievementManager(AchievementCategory category)
    {
        for(int i = 0; i < subAchievementManagers.Count; i++)
        {
            if(subAchievementManagers[i].category == category)
            {
                return subAchievementManagers[i];
            }
        }
        return null;
    }
    void OnLoadedScene(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "InGame")
        {
            GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
            GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);   
        }
        else if(scene.name == "Lobby")
        {
            GameEventBus.Subscribe<LevelUpSlimeEvent>(OnLevelUpSlimeEvent);
        }
        
    }


    void OnUnloadedScene(Scene scene)
    {    
        if(scene.name == "InGame")
        {
            GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
            GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
            GameEventBus.Unsubscribe<ClearStageEvent>(OnClearStageEvent);
            GameEventBus.Unsubscribe<TryStageEvent>(OnTryStageEvent);
        }
        else if(scene.name == "Lobby")
        {            
            GameEventBus.Unsubscribe<LevelUpSlimeEvent>(OnLevelUpSlimeEvent);
        }
        
    }


    public void Achieve(AchievementType achievementType)
    {
        for(int i = 0; i < subAchievementManagers.Count; i++)
        {
            subAchievementManagers[i].Achieve(achievementType);
        }
    }


    void OnDestroyedStoneEvent(DestroyedStoneEvent e)
    {
        Achieve(AchievementType.DestroyStone);
    }
    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        Achieve(AchievementType.KillEnemy);
    }
    void OnLevelUpSlimeEvent(LevelUpSlimeEvent e)
    {        
        Achieve(AchievementType.LevelUpSlime);
    }
    private void OnTryStageEvent(TryStageEvent @event)
    {
        Achieve(AchievementType.TryStage);
    }
    private void OnClearStageEvent(ClearStageEvent @event)
    {        
        Achieve(AchievementType.ClearStage);
    }
}
public enum AchievementCategory
{
    Daily,
    Cumulative
}
public enum AchievementType
{
    DestroyStone,
    KillEnemy,
    ClearStage,
    Login,
    LevelUpSlime,
    TryStage,
    DrawSlime,
    DrawEquipment,
    Watch_Ad
}

public abstract class AchievementData
{
    public AchievementType type;
    public ConditionData conditionData;
    public RewardData rewardData;
    public string Title => $"{type}";
    public string Desc() => $"{GetUserAchievement().value}/{GetGoal()}";
    public float fillAmount => (float)GetUserAchievement().value/(float)GetGoal();

    public abstract UserAchievement GetUserAchievement();
    public abstract bool CheckCanClear();
    public abstract int GetGoal();
}


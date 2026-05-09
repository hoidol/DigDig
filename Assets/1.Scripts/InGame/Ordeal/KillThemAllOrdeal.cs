using TMPro;
using UnityEngine;

public class KillThemAllOrdeal : Ordeal
{
    int targetCount;
    int totalSpawned;
    bool patternStopped;
    public TMP_Text remainEnemyCountText; // 남은 적 : {처치 수}/{목표값}

    public override void StartOrdeal(OrdealData ordealData)
    {
        base.StartOrdeal(ordealData);
        targetCount = int.Parse(ordealData.data);
        totalSpawned = 0;
        patternStopped = false;
        enemyPattern.onSpawned = OnEnemySpawned;
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
        remainEnemyCountText.text = $"처치 : {targetCount}/{targetCount}";
    }

    void OnEnemySpawned()
    {
        totalSpawned++;
        if (totalSpawned >= targetCount)
        {
            enemyPattern.onSpawned = null;
            enemyPattern.EndPattern();
            patternStopped = true;
        }
    }

    void OnEnemyDead(EnemyDeadEvent e)
    {
        if (remainEnemyCountText != null)
            remainEnemyCountText.text = $"처치 : {EnemyManager.Instance.ActiveEnemyCount}/{totalSpawned}";
        if (patternStopped && EnemyManager.Instance.ActiveEnemyCount == 0)
            EndOrdeal();
    }

    public override void EndOrdeal()
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
        enemyPattern.onSpawned = null;
        if (!patternStopped)
            enemyPattern.EndPattern();
        base.EndOrdeal();
    }
}

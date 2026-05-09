using TMPro;
using UnityEngine;

public class KillEnemiesOrdeal : Ordeal
{
    int killCount;
    int targetCount;

    public TMP_Text remainEnemyCountText; // 처치 수 : {처치 수}/{목표값}

    public override void StartOrdeal(OrdealData ordealData)
    {
        base.StartOrdeal(ordealData);
        targetCount = int.Parse(ordealData.data);
        killCount = 0;
        remainEnemyCountText.text = $"처치 : {killCount}/{targetCount}";
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
    }

    void OnEnemyDead(EnemyDeadEvent e)
    {
        killCount++;
        if (remainEnemyCountText != null)
            remainEnemyCountText.text = $"처치 : {killCount}/{targetCount}";
        if (killCount >= targetCount)
            EndOrdeal();
    }

    public override void EndOrdeal()
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
        enemyPattern.EndPattern();
        base.EndOrdeal();
    }
}

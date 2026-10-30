using UnityEngine;

public class EliteWave : BattleWave
{

    public override void StartWave(int wIdx, DefenceType dType)
    {
        base.StartWave(wIdx, dType);
        Enemy enemyPrefab = GameManager.Instance.stageData.GetEnemyPrefab(EnemyType.Elite, waveDefenceType);
        Enemy enemy = EnemySpawner.Instance.Instantiate(enemyPrefab);
        enemy?.Spawn(EnemySpawner.Instance.GetSpawnPosition(), wIdx);
    }
}
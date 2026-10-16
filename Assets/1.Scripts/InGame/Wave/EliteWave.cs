using UnityEngine;

public class EliteWave : BattleWave
{
    public override void StartWave(int phase)
    {
        base.StartWave(phase);
        Enemy enemyPrefab = GameManager.Instance.stageData.GetEnemyPrefab(EnemyType.Elite);
        Enemy enemy = EnemySpawner.Instance.Instantiate(enemyPrefab);
        enemy?.Spawn(EnemySpawner.Instance.GetSpawnPosition());
    }
}
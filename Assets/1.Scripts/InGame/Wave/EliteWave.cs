using UnityEngine;

public class EliteWave : BattleWave
{

    public override void StartWave(int wIdx, DefenceType dType)
    {
        base.StartWave(wIdx, dType);
        // 엘리트·보스는 wave 방어 타입과 상관없이 항상 Normal 방어 타입
        Enemy enemyPrefab = GameManager.Instance.stageData.GetEnemyPrefab(EnemyType.Elite);
        Enemy enemy = EnemySpawner.Instance.Instantiate(enemyPrefab);
        enemy?.Spawn(EnemySpawner.Instance.GetSpawnPosition(), wIdx);
    }
}
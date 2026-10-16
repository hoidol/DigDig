using System;
using System.Threading;
using Cysharp.Threading.Tasks;

// 웨이브 동안 spawnInterval마다 1마리씩 경로 시작점에 소환
// 소환량은 고정, 난이도는 적 체력(Enemy.GetHp)으로만 올린다
public class PhaseEnemySpawnPattern : SpawnPattern
{
    public EnemyType enemyType = EnemyType.Normal;
    // public float spawnInterval = 1f;

    CancellationTokenSource cts;

    public override void StartPattern()
    {
        EndPattern();
        cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        SpawnLoop(cts.Token).Forget();
    }

    // 시작 즉시 1마리, 이후 spawnInterval마다 1마리. ENEMY_MAX_SPAWN_TIME이 지나면 스폰 중지
    async UniTaskVoid SpawnLoop(CancellationToken token)
    {
        float elapsed = 0f;
        while (elapsed < GameSetting.ENEMY_MAX_SPAWN_TIME)
        {
            Spawn(enemyType);

            if (await UniTask.Delay(TimeSpan.FromSeconds(GameSetting.ENEMY_SPAWN_INTERVAL), cancellationToken: token).SuppressCancellationThrow())
                return;
            elapsed += GameSetting.ENEMY_SPAWN_INTERVAL;
        }
    }

    public void Spawn(EnemyType type)
    {
        Enemy enemyPrefab = GameManager.Instance.stageData.GetEnemyPrefab(type, GameManager.Instance.phaseDefenceType);
        Enemy enemy = EnemySpawner.Instance.Instantiate(enemyPrefab);
        enemy.Spawn(EnemySpawner.Instance.GetSpawnPosition());
    }

    public override void EndPattern()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}

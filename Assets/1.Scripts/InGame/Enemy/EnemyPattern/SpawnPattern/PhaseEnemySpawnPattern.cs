using System;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

// 웨이브 동안 spawnInterval마다 1마리씩 경로 시작점에 소환
// 소환량은 고정, 난이도는 적 체력(Enemy.GetHp)으로만 올린다
public class PhaseEnemySpawnPattern : SpawnPattern
{
    // public EnemyType enemyType = EnemyType.Normal;
    // public float spawnInterval = 1f;

    CancellationTokenSource cts;

    public override void StartPattern()
    {
        EndPattern();
        cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        SpawnLoop(cts.Token).Forget();
    }

    // 시작 즉시 1마리, 이후 spawnInterval마다 1마리. 웨이브 종료(취소)까지 계속
    async UniTaskVoid SpawnLoop(CancellationToken token)
    {
        while (true)
        {
            Spawn(EnemyType.Normal);

            if (await UniTask.Delay(TimeSpan.FromSeconds(GameSetting.ENEMY_SPAWN_INTERVAL), cancellationToken: token).SuppressCancellationThrow())
                return;
        }
    }

    public void Spawn(EnemyType type)
    {
        // Enemy enemyPrefab = GameManager.Instance.stageData.GetEnemyPrefab(type, GameManager.Instance.waveDefenceType);
        // Debug.Log($"PhaseEnemySpawnPattern Spawn type {type}, GameManager.Instance.phaseDefenceType {GameManager.Instance.waveDefenceType}");
        // Enemy enemy = EnemySpawner.Instance.Instantiate(enemyPrefab);
        // enemy.Spawn(EnemySpawner.Instance.GetSpawnPosition());
    }

    public override void EndPattern()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class BattleWave : Wave
{

    public override void StartWave(int wIdx, DefenceType dType)
    {
        base.StartWave(wIdx, dType);

        StageData stageData = GameManager.Instance.stageData;
        float hp = stageData.enemyHp + (stageData.enemyHp * (1f + stageData.addHpRatioPerWave) * (float)waveIdx);
        // float hp = stageData.enemyHp + stageData.enemyHp * ((1 + stageData.addHpRatioPerWave) * wIdx); //10,23,36,49
        // float hp = stageData.enemyHp * (1 + stageData.addHpRatioPerWave * waveIdx);//13,16,19,22

        // 체력 비교 (Normal_0: enemyHp 10, addHpRatioPerWave 0.3, multiHpPerWave 1.1),
        // 최종 = 식 × multiHpPerWave^wave
        // A = stageData.enemyHp + (stageData.enemyHp * (1f + stageData.addHpRatioPerWave) * (float)wave);
        // B = stageData.enemyHp + stageData.enemyHp * ((1 + stageData.addHpRatioPerWave) * wave);
        // C = stageData.enemyHp * (1 + stageData.addHpRatioPerWave * wave);
        // wave |      A |      B |      C
        //   0  |   10.0 |   10.0 |   10.0
        //   1  |   25.3 |   25.3 |   14.3
        //   2  |   43.6 |   43.6 |   19.4
        //   3  |   65.2 |   65.2 |   25.3
        //   4  |   90.8 |   90.8 |   32.2
        //   5  |  120.8 |  120.8 |   40.3
        //   6  |  155.9 |  155.9 |   49.6
        //   7  |  196.8 |  196.8 |   60.4
        //   8  |  244.4 |  244.4 |   72.9
        //   9  |  299.5 |  299.5 |   87.2
        //  10  |  363.1 |  363.1 |  103.7
        //  15  |  856.3 |  856.3 |  229.7
        //  19  | 1571.8 | 1571.8 |  409.8

        hp = hp * Mathf.Pow(stageData.multiHpPerWave, waveIdx);
        Debug.Log($"BattleWave StartWave {wIdx} 최종 체력 {hp} addHpRatioPerWave {stageData.addHpRatioPerWave} multiHpPerWave {stageData.multiHpPerWave}");

        cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        SpawnLoop(cts.Token).Forget();
    }


    CancellationTokenSource cts;

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
        Enemy enemyPrefab = GameManager.Instance.stageData.GetEnemyPrefab(type, waveDefenceType);
        // Debug.Log($"PhaseEnemySpawnPattern Spawn type {type}, GameManager.Instance.phaseDefenceType {waveDefenceType}");
        Enemy enemy = EnemySpawner.Instance.Instantiate(enemyPrefab);
        enemy.Spawn(EnemySpawner.Instance.GetSpawnPosition(), waveIdx);
    }

    public virtual void EndSpawnEnemy()
    {
    }


    // 타이머 종료(또는 스킵) → 스폰 중지 → 휴식으로. 필드에 남은 적은 그대로 진행
    public override void EndWave()
    {
        base.EndWave();
        cts?.Cancel();
        cts?.Dispose();
        cts = null;

    }


}

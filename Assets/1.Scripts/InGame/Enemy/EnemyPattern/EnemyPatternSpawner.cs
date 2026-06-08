using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyPatternSpawner
{
    public EnemyPatternData enemyPatternData;
    //public Action onSpawned;

    CancellationTokenSource cts;

    public virtual void StartPattern(EnemyPatternData enemyPatternData)
    {
        EndPattern();
        Debug.Log("EnemyPattern StartPattern");
        this.enemyPatternData = enemyPatternData;
        cts = new CancellationTokenSource();
        foreach (var spawnData in enemyPatternData.enemySpawnPatternDatas)
            SpawnLoop(spawnData, cts.Token).Forget();
    }

    async UniTaskVoid SpawnLoop(EnemySpawnPatternData spawnData, CancellationToken token)
    {
        while (true)
        {
            float wait = Random.Range(spawnData.intervalRange.x, spawnData.intervalRange.y);
            await UniTask.Delay(TimeSpan.FromSeconds(wait), cancellationToken: token);

            Debug.Log("EnemyPattern SpawnLoop 적 생성하기");
            if (EnemyManager.Instance.ActiveEnemyCount >= StageData.MAX_ENEMY_COUNT) continue;

            int count = Random.Range(spawnData.countRange.x, spawnData.countRange.y);
            for (int i = 0; i < count; i++)
            {
                if (EnemyManager.Instance.ActiveEnemyCount >= StageData.MAX_ENEMY_COUNT) break;

                Spawn(spawnData.enemyType);
            }
        }
    }


    public void Spawn(EnemyType type)
    {
        Debug.Log($"EnemyPattern Spawn 적 생성하기 {type}");
        EnemyData enemyData = EnemyManager.GetEnemyData(type);
        var sorted = Player.Instance.tileCheckers.OrderBy(c => c.TileCount()).ToList();
        var bestChecker = sorted.Take(2).OrderBy(_ => Random.value).First();
        Vector2 center = bestChecker.transform.position;
        Vector2 rPoint = center + Random.insideUnitCircle * 5f;

        Vector2 offset = rPoint - (Vector2)Player.Instance.transform.position;
        Vector2Int dir = Mathf.Abs(offset.x) > Mathf.Abs(offset.y)
            ? (offset.x > 0 ? Vector2Int.right : Vector2Int.left)
            : (offset.y > 0 ? Vector2Int.up : Vector2Int.down);

        Vector2 spawnPoint;

        if (enemyData.size == Vector2Int.one && bestChecker.tile != null && Random.Range(0f, 100f) < 70)
        {
            Debug.Log($"best Checker 생성 시도 {bestChecker.name}");
            Vector2Int tileIdx = MapManager.PositionToIndex(bestChecker.tile.Transform.position);

            Vector2Int? found = FindEmptyInDir(tileIdx, dir, 4)
                             ?? FindEmptyInDir(tileIdx, -dir, 4);

            if (!found.HasValue)
            {
                Vector2Int rIdx = MapManager.PositionToIndex(rPoint);
                if (MapManager.CheckEmpty(rIdx)) found = rIdx;
            }

            if (!found.HasValue)
            {
                Debug.Log("빈칸이 없어서 생성 못함");
                return; // 빈칸 없으면 생성 안함
            }
            spawnPoint = MapManager.IndexToPosition(found.Value);
        }
        else
        {

            spawnPoint = rPoint + (Vector2)Player.Instance.transform.position;
            Debug.Log($"랜덤으로 생성 시도 {spawnPoint}");
            if (!MapManager.CheckEmpty(MapManager.PositionToIndex(spawnPoint)))
            {
                return;
            }

            spawnPoint = MapManager.SnappedPosition(spawnPoint);
        }

        Enemy enemy = EnemyManager.Instance.Instantiate(type);
        enemy?.Spawn(spawnPoint);
    }

    Vector2Int? FindEmptyInDir(Vector2Int startIdx, Vector2Int dir, int steps)
    {
        for (int i = 1; i <= steps; i++)
        {
            Vector2Int candidate = startIdx + dir * i;
            if (MapManager.CheckEmpty(candidate)) return candidate;
        }
        return null;
    }


    public virtual void EndPattern()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}

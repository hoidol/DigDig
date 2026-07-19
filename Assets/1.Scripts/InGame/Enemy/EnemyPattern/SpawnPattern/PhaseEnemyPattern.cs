using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class PhaseEnemyPattern : SpawnPattern
{
    public EnemyPatternData enemyPatternData;
    //public Action onSpawned;

    CancellationTokenSource cts;

    void Awake()
    {
        GameEventBus.Subscribe<PhaseStartEvent>(OnPhaseStartEvent);
    }

    void OnPhaseStartEvent(PhaseStartEvent e)
    {
        EndPattern();
        Debug.Log("EnemyPattern StartPattern");

        this.enemyPatternData = GameManager.Instance.stageData.phaseDatas[e.phaseIdx].enemyPatternData;
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

            int count = Random.Range(spawnData.countRange.x, spawnData.countRange.y);
            for (int i = 0; i < count; i++)
            {
                Spawn(spawnData.enemyType);
            }
        }
    }


    public void Spawn(EnemyType type)
    {
        EnemyData enemyData = EnemyManager.GetEnemyData(type);
        var sorted = Player.Instance.tileCheckers.OrderBy(c => c.TileCount()).ToList();
        var bestChecker = sorted.Take(2).OrderBy(_ => Random.value).First();
        Vector2 center = bestChecker.transform.position;
        Vector2 rPoint = center + Random.insideUnitCircle * 5f;

        //dir 방향
        Vector2 offset = rPoint - (Vector2)Player.Instance.transform.position;
        Vector2Int dir = Mathf.Abs(offset.x) > Mathf.Abs(offset.y)
            ? (offset.y > 0 ? Vector2Int.up : Vector2Int.down)
            : (offset.x > 0 ? Vector2Int.right : Vector2Int.left);


        Vector2Int[,] spawnTileArray = null;
        bool canSpawn = true;
        if (bestChecker.tile != null && Random.Range(0f, 100f) < 70)
        {
            Vector2Int startTileArr = MapManager.PositionToTileIndex(bestChecker.tile.Transform.position);
            if (!MapManager.GetTileArray(startTileArr, enemyData.size, out spawnTileArray))
            {
                if (!FindEmptyInDir(startTileArr, enemyData.size, dir, 4, out spawnTileArray))
                {
                    if (!FindEmptyInDir(startTileArr, enemyData.size, -dir, 4, out spawnTileArray))
                    {
                        canSpawn = false;
                    }
                }
            }
        }
        else
        {
            Vector2Int startTileArr = MapManager.PositionToTileIndex(rPoint);
            if (!MapManager.GetTileArray(startTileArr, enemyData.size, out spawnTileArray))
            {
                canSpawn = false;
            }

        }

        if (canSpawn)
        {

            Enemy enemyPrefab = GameManager.Instance.stageData.GetEnemyPrefab(type);
            Enemy enemy = EnemyManager.Instance.Instantiate(enemyPrefab);
            enemy?.Spawn(spawnTileArray);
        }

    }
    bool FindEmptyInDir(Vector2Int startIdx, Vector2Int size, Vector2Int dir, int steps, out Vector2Int[,] spawnTileArray)
    {
        spawnTileArray = new Vector2Int[size.x, size.y];
        for (int i = 1; i <= steps; i++)
        {
            Vector2Int candidate = startIdx + dir * i;
            if (MapManager.GetTileArray(candidate, size, out spawnTileArray)) return true;
        }
        return false;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.E))
        {
            Spawn(EnemyType.Melee);
        }
#endif
    }

    public virtual void EndPattern()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}

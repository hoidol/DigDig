using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossSpawner : SpecialEnemySpawner
{
    List<UnbreakableStone> unbreakableStones;
    void Start()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }
    Boss boss;
    public override void Spawn()
    {
        Boss boss = Instantiate(GameManager.Instance.stageData.boss);

        // Vector2Int tileIndex = MapManager.PositionToTileIndex(Character.Instance.transform.position);
        // Vector2Int[,] tileIndexArr = MapManager.GetIndexArray(tileIndex, boss.Size);

        var bestChecker = Character.Instance.tileCheckers.OrderBy(c => c.TileCount()).First();
        Vector2 bestCheckerCenter = bestChecker.transform.position;

        Vector2 rPoint = bestCheckerCenter + Random.insideUnitCircle * 5f;

        Vector2Int center = MapManager.PositionToTileIndex(rPoint);
        unbreakableStones = MapManager.Instance.MakeUnbreakableStone(center, 30, 20);

        // MapManager.GetTileArray(center, boss.enemyData.size, out Vector2Int[,] spawnTileArray);

        
        boss.Spawn(EnemySpawner.Instance.GetSpawnPosition());
        BossCanvas.Instance.SetBoss(boss);
        GameEventBus.Publish(new BossSpawnEvent(boss));
    }

    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        if (e.enemy == boss)
        {
            EndSpawn();
        }
    }



    public override void EndSpawn()
    {
        for (int i = 0; i < unbreakableStones.Count; i++)
        {
            unbreakableStones[i].Destroy();
        }
    }
}

public class BossSpawnEvent
{
    public Boss boss;
    public BossSpawnEvent(Boss boss)
    {
        this.boss = boss;
    }
}
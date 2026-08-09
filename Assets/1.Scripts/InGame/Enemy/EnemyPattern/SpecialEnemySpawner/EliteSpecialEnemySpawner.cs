using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class EliteSpecialEnemySpawner : SpecialEnemySpawner
{
    List<UnbreakableStone> unbreakableStones;
    EliteEnemy eliteEnemy;
    void Start()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    public override void Spawn()
    {
        Vector2Int center = MapManager.PositionToTileIndex(Character.Instance.transform.position);

        unbreakableStones = MapManager.Instance.MakeUnbreakableStone(center, 20, 10);


        Enemy eliteEnemyPrefab = GameManager.Instance.stageData.GetEnemyPrefab(EnemyType.Elite);
        EnemyData enemyData = EnemyManager.GetEnemyData(eliteEnemyPrefab.enemyType);
        var sorted = Character.Instance.tileCheckers.OrderBy(c => c.TileCount()).ToList();
        var bestChecker = sorted.Take(2).OrderBy(_ => Random.value).First();
        Vector2 bestCheckerCenter = bestChecker.transform.position;
        Vector2 rPoint = bestCheckerCenter + Random.insideUnitCircle * 5f;

        Vector2Int startTileArr = MapManager.PositionToTileIndex(rPoint);
        MapManager.GetTileArray(startTileArr, enemyData.size, out Vector2Int[,] spawnTileArray);

        eliteEnemy = EnemySpawner.Instance.Instantiate(eliteEnemyPrefab) as EliteEnemy;
        eliteEnemy?.Spawn(spawnTileArray);
        GameEventBus.Publish(new EliteSpawnEvent(eliteEnemy));
    }

    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        if (e.enemy == eliteEnemy)
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

public class EliteSpawnEvent
{
    public EliteEnemy eliteEnemy;
    public EliteSpawnEvent(EliteEnemy eliteEnemy)
    {
        this.eliteEnemy = eliteEnemy;
    }
}

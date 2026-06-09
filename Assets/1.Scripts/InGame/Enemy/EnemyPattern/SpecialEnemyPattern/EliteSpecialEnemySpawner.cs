using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class EliteSpecialEnemySpawner : SpecialEnemySpawner
{
    public EliteEnemy eliteEnemyPrefab;
    public override void Spawn()
    {
        EnemyData enemyData = EnemyManager.GetEnemyData(eliteEnemyPrefab.enemyType);
        var sorted = Player.Instance.tileCheckers.OrderBy(c => c.TileCount()).ToList();
        var bestChecker = sorted.Take(2).OrderBy(_ => Random.value).First();
        Vector2 center = bestChecker.transform.position;
        Vector2 rPoint = center + Random.insideUnitCircle * 5f;

        Vector2Int startTileArr = MapManager.PositionToTileIndex(rPoint);
        MapManager.GetTileArray(startTileArr, enemyData.size, out Vector2Int[,] spawnTileArray);
        
        EliteEnemy enemy = EnemyManager.Instance.Instantiate(eliteEnemyPrefab.enemyType) as EliteEnemy;
        enemy?.Spawn(spawnTileArray);    
    }

    public override void EndSpawn()
    {
        
    }
}

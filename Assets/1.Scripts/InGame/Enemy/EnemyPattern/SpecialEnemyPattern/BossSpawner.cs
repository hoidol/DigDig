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
    EliteEnemy eliteEnemy;
    void Start()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }
   
    public override void Spawn()
    {

        Boss boss = Instantiate(GameManager.Instance.stageData.boss);

        Vector2Int tileIndex = MapManager.PositionToTileIndex(Player.Instance.transform.position);
        Vector2Int[,] tileIndexArr = MapManager.GetIndexArray(tileIndex, boss.Size);



        var bestChecker = Player.Instance.tileCheckers.OrderBy(c => c.TileCount()).First();
        Vector2 bestCheckerCenter = bestChecker.transform.position;


        Vector2 rPoint = bestCheckerCenter + Random.insideUnitCircle * 5f;


        Vector2Int center = MapManager.PositionToTileIndex(rPoint); 
        unbreakableStones = MapManager.Instance.MakeUnbreakableStone(center,60,40);

        MapManager.GetTileArray(center, boss.enemyData.size, out Vector2Int[,] spawnTileArray);

        boss.Spawn(tileIndexArr);
        BossCanvas.Instance.SetBoss(boss);
    }

     void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        if(e.enemy == eliteEnemy)
        {
            EndSpawn();
        }
    }



    public override void EndSpawn()
    {
        for(int i = 0; i < unbreakableStones.Count; i++)
        {
            unbreakableStones[i].Destroyed(false);
        }
    }
}

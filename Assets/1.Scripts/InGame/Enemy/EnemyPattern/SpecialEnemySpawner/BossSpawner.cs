using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossSpawner : SpecialEnemySpawner
{
    public override void Spawn()
    {
        Boss boss = Instantiate(GameManager.Instance.stageData.boss);
        Vector2 spawnPos = new Vector2(0,EnemySpawner.Instance.GetSpawnTopPosition().y);
        
        boss.Spawn(spawnPos);
        BossCanvas.Instance.SetBoss(boss);
        GameEventBus.Publish(new BossSpawnEvent(boss));
    }


    public override void EndSpawn()
    {
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
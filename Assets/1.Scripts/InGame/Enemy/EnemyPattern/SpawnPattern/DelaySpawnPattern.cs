using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DelaySpawnPattern : SpawnPattern
{
    public SpecialEnemySpawner[] specialEnemySpawners;
    public float delay = 0f; // 웨이브 시작 후 Spawn까지 대기(초)


    public override void StartPattern()
    {
        
        DelaySpawn().Forget();
    }
    async UniTaskVoid DelaySpawn()
    {
        if (await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow())
            return;
        for(int i = 0; i < specialEnemySpawners.Length; i++)
        {
            specialEnemySpawners[i].Spawn();
        }      
    }

}

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StartWaveSpawnPattern : SpawnPattern
{

    // public SpecialEnemySpawner specialEnemySpawner;
    // public int[] phaseIdxs;
    public int phaseIdx;
    public SpecialEnemySpawner[] specialEnemySpawners;
    // public float delay = 0f; // 웨이브 시작 후 Spawn까지 대기(초)

    void Start()
    {
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
        specialEnemySpawners = GetComponentsInChildren<SpecialEnemySpawner>();
    }

    void OnWaveStartEvent(WaveStartEvent e)
    {
        Debug.Log($"StartWaveSpawnPattern OnWaveStartEvent {e.phaseIdx}");
        if (phaseIdx != e.phaseIdx)
            return;


        DelaySpawn().Forget();
    }

    async UniTaskVoid DelaySpawn()
    {
        // if (await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow())
        //     return;
        // specialEnemySpawner.Spawn();
        for(int i = 0; i < specialEnemySpawners.Length; i++)
        {
            specialEnemySpawners[i].Spawn();
        }

    }

}

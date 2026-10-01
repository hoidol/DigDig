using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class EliteEnemyPattern : SpawnPattern
{
    int[] spawnPhaseIdxs = {2,5,8};

    void Awake()
    {
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
    }
    void OnWaveStartEvent(WaveStartEvent e)
    {
        Debug.Log("EliteEnemyPattern OnWaveStartEvent");
        if (!spawnPhaseIdxs.Contains(e.phaseIdx))
            return;
        Spawn(EnemyType.Elite);
    }



    public void Spawn(EnemyType type)
    {
        Enemy enemyPrefab = GameManager.Instance.stageData.GetEnemyPrefab(type);
        Enemy enemy = EnemySpawner.Instance.Instantiate(enemyPrefab);
        enemy?.Spawn(EnemySpawner.Instance.GetSpawnPosition());
    }
    
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.E))
        {
            Spawn(EnemyType.Elite);
        }
#endif
    }

}

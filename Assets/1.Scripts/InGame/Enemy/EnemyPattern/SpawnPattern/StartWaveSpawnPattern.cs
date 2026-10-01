using System.Linq;
using UnityEngine;

public class StartWaveSpawnPattern : SpawnPattern
{

    public SpecialEnemySpawner specialEnemySpawner;
    public int[] phaseIdxs;

    public override void StartGame()
    {
        // GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
    }
    void Start()
    {
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
    }

    void OnWaveStartEvent(WaveStartEvent e)
    {
        Debug.Log($"StartWaveSpawnPattern OnWaveStartEvent {e.phaseIdx}");
        if (phaseIdxs.Contains(e.phaseIdx) == false)
            return;
        specialEnemySpawner.Spawn();
    }

}
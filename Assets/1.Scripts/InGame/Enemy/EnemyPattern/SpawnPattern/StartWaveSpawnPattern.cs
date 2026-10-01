using UnityEngine;

public class StartWaveSpawnPattern : SpawnPattern
{

    public SpecialEnemySpawner specialEnemySpawner;

    public override void StartGame()
    {
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
    }

    void OnWaveStartEvent(WaveStartEvent e)
    {
        specialEnemySpawner.Spawn();
    }

}
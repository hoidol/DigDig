using UnityEngine;

public class StartNightSpawnPattern : SpawnPattern
{

    public SpecialEnemySpawner specialEnemySpawner;

    public override void StartGame()
    {
        GameEventBus.Subscribe<WaveStartEvent>(OnNightStartEvent);
    }

    void OnNightStartEvent(WaveStartEvent e)
    {
        specialEnemySpawner.Spawn();
    }

}
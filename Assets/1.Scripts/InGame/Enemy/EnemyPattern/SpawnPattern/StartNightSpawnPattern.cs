using UnityEngine;

public class StartNightSpawnPattern : SpawnPattern
{

    public SpecialEnemySpawner specialEnemySpawner;

    public override void StartGame()
    {
        GameEventBus.Subscribe<NightStartEvent>(OnNightStartEvent);
    }

    void OnNightStartEvent(NightStartEvent e)
    {
        specialEnemySpawner.Spawn();
    }

}
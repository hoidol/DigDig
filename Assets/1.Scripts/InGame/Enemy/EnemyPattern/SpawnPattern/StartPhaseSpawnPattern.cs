using UnityEngine;

public class StartPhaseSpawnPattern : SpawnPattern
{
    public int[] phaseIdxs;
    public SpecialEnemySpawner[] specialEnemySpawners;
    void Awake()
    {
        GameEventBus.Subscribe<BreakStartEvent>(OnDayStartEvent);
    }

    void OnDayStartEvent(BreakStartEvent e)
    {

        for (int i = 0; i < phaseIdxs.Length; i++)
        {
            if (phaseIdxs[i] == e.phaseIdx)
            {
                specialEnemySpawners[i].Spawn();
            }
        }

    }
}
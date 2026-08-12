using UnityEngine;

public class StartPhaseSpawnPattern : SpawnPattern 
{
    public int[] phaseIdxs;
    public SpecialEnemySpawner[] specialEnemySpawners;
    void Awake()
    {
        GameEventBus.Subscribe<DayStartEvent>(OnDayStartEvent);
    }

    void OnDayStartEvent(DayStartEvent e)
    {

        for(int i = 0; i < phaseIdxs.Length; i++)
        {
            if(phaseIdxs[i] == e.phaseIdx)
            {
                specialEnemySpawners[i].Spawn();       
            }
        }
         
    }
}
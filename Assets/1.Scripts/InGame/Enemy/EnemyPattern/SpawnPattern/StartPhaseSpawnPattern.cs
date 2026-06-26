using UnityEngine;

public class StartPhaseSpawnPattern : SpawnPattern 
{
    public int[] phaseIdxs;
    public SpecialEnemySpawner[] specialEnemySpawners;
    void Awake()
    {
        GameEventBus.Subscribe<PhaseStartEvent>(OnPhaseStartEvent);
    }

    void OnPhaseStartEvent(PhaseStartEvent e)
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
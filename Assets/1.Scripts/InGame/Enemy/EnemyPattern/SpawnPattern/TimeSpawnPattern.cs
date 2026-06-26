using UnityEngine;

public class TimeSpawnPattern : SpawnPattern 
{

    int spawnCount = 0;
    public float[] times;
    public SpecialEnemySpawner[] specialEnemySpawners;
    

    public void Update()
    {
        if(spawnCount >= times.Length)
            return; 
            
        if(GameManager.Instance.gameTimer >= times[spawnCount])
        {
            specialEnemySpawners[spawnCount].Spawn();
            spawnCount++;
        }
    }
}
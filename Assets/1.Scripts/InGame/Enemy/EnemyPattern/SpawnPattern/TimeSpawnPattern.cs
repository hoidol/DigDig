using UnityEngine;

public class TimeSpawnPattern : SpawnPattern
{

    int spawnCount = 0;
    public float[] times;
    public SpecialEnemySpawner[] specialEnemySpawners;

    public override void StartGame()
    {
        spawnCount = 0;
    }

    public void Update()
    {
        if (spawnCount >= times.Length)
            return;

        if (GameManager.Instance.gameTimer >= times[spawnCount])
        {
            Debug.Log("TimeSpawnPattern Spawn");
            specialEnemySpawners[spawnCount].Spawn();
            spawnCount++;
        }
    }
}
using UnityEngine;

public class TimeSpawnPattern : SpawnPattern
{

    int spawnCount = 0;
    public float[] times;
    public SpecialEnemySpawner specialEnemySpawner;

    public override void StartGame()
    {
        spawnCount = 0;
    }

    public void Update()
    {
        if (!GameManager.Instance.isPlaying)
            return;

        if (GameManager.Instance.gameTimer >= times[spawnCount])
        {
            Debug.Log("TimeSpawnPattern Spawn");
            specialEnemySpawner.Spawn();
            spawnCount++;
        }
    }
}
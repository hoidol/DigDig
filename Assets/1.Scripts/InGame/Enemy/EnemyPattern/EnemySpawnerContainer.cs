using UnityEngine;

public class EnemySpawnerContainer : MonoBehaviour
{
    public SpawnPattern[] spawnPatterns;
    public BossSpawner bossSpawner;

    void Awake()
    {
        spawnPatterns = GetComponentsInChildren<SpawnPattern>();
        bossSpawner = GetComponentInChildren<BossSpawner>();
    }

}
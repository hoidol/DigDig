using UnityEngine;

public class EnemySpawnerContainer : MonoBehaviour 
{
    public SpawnPattern[] spawnPatterns;
    
    void Awake()
    {
        spawnPatterns = GetComponentsInChildren<SpawnPattern>();
        
    }

}
using System;
using System.Collections;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPattern : MonoBehaviour
{
    [SerializeField] public EnemyPatternData enemyPatternData;

    public Action onSpawned;
    readonly List<Coroutine> spawnCoroutines = new();

    public virtual void StartPattern(EnemyPatternData enemyPatternData)
    {
        spawnCoroutines.Clear();
        this.enemyPatternData = enemyPatternData;
        foreach (var spawnData in enemyPatternData.enemySpawnPatternDatas)
            spawnCoroutines.Add(StartCoroutine(CoSpawn(spawnData)));
    }

    IEnumerator CoSpawn(EnemySpawnPatternData spawnData)
    {
        while (true)
        {
            float wait = Random.Range(spawnData.intervalRange.x, spawnData.intervalRange.y);
            yield return new WaitForSeconds(wait);

            if (EnemyManager.Instance.ActiveEnemyCount >= StageData.MAX_ENEMY_COUNT) continue;

            int count = Random.Range(spawnData.countRange.x, spawnData.countRange.y);
            for (int i = 0; i < count; i++)
            {
                if (EnemyManager.Instance.ActiveEnemyCount >= StageData.MAX_ENEMY_COUNT) break;
                GameManager.Instance.Spawn(spawnData.enemyType);
                onSpawned?.Invoke();
            }
        }
    }

    public virtual void EndPattern()
    {
        foreach (var cor in spawnCoroutines)
        {
            if (cor != null) StopCoroutine(cor);
        }
        spawnCoroutines.Clear();
        Destroy(gameObject);
    }
}

public enum EnemyPatternType
{
    Weak,
    Middle,
    Strong
}

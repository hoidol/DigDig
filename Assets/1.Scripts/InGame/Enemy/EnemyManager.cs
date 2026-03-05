using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoSingleton<EnemyManager>
{
    public Enemy[] enemiePrefabs;

    public Dictionary<EnemyType, List<Enemy>> enemies = new Dictionary<EnemyType, List<Enemy>>();
    private void Awake()
    {
        enemiePrefabs = Resources.LoadAll<Enemy>("Enemy");
    }

    public Enemy GetEnemy(EnemyType enemyType)
    {
        if (!enemies.ContainsKey(enemyType))
        {
            enemies.Add(enemyType, new List<Enemy>());
        }

        for (int i = 0; i < enemies[enemyType].Count; i++)
        {
            if (enemies[enemyType][i].gameObject.activeSelf)
                continue;
            enemies[enemyType][i].gameObject.SetActive(true);
            return enemies[enemyType][i];
        }

        Enemy prefab = GetEnemyPrefab(enemyType);
        Enemy enemy = Instantiate(prefab);
        enemies[enemyType].Add(enemy);
        return enemy;
    }

    Enemy GetEnemyPrefab(EnemyType enemyType)
    {
        for (int i = 0; i < enemiePrefabs.Length; i++)
        {
            if (enemiePrefabs[i].enemyType == enemyType)
            {
                return enemiePrefabs[i];
            }
        }
        return null;
    }
}

using System;
using System.Collections.Generic;
using LayerLab.ArtMakerUnity;
using UnityEngine;

public class EnemySpawner : MonoSingleton<EnemySpawner>
{
    readonly Dictionary<EnemyType, Stack<Enemy>> pool = new(); // 적 종류 별 풀링

    [field: SerializeField] public int ActiveEnemyCount { get; private set; }

    public readonly HashSet<Enemy> activeEnemies = new();
    void Start()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(EnemyDeadEventListener);
    }

    public Enemy Instantiate(Enemy prefab)
    {
        // EnemyData data = EnemyManager.enemyDataDic[prefab.enemyType];

        if (!pool.ContainsKey(prefab.enemyType))
            pool[prefab.enemyType] = new Stack<Enemy>();

        Enemy enemy = pool[prefab.enemyType].Count > 0
            ? pool[prefab.enemyType].Pop()
            : GameObject.Instantiate(prefab);


        enemy.gameObject.SetActive(true);
        activeEnemies.Add(enemy);
        ActiveEnemyCount++;
        return enemy;
    }

    void ReleaseEnemy(Enemy enemy)
    {
        if (!pool.ContainsKey(enemy.enemyType))
            pool[enemy.enemyType] = new Stack<Enemy>();

        activeEnemies.Remove(enemy);
        enemy.gameObject.SetActive(false);
        pool[enemy.enemyType].Push(enemy);
        ActiveEnemyCount = Mathf.Max(0, ActiveEnemyCount - 1);
    }

    public void EnemyDeadEventListener(EnemyDeadEvent e)
    {
        ReleaseEnemy(e.enemy);
    }

    // public Enemy GetEnemyInTileIndex(Vector2Int tileIdx)
    // {
    //     foreach (Enemy enemy in activeEnemies)
    //     {
    //         foreach (Vector2Int idx in enemy.TileIndexArr)
    //         {
    //             if (idx == tileIdx) return enemy;
    //         }
    //     }
    //     return null;
    // }

    public Vector2 GetSpawnPosition(float howFar = -1)
    {
        if (howFar < 0)
        {
            howFar = (CameraManager.Instance.mainCamera.orthographicSize * CameraManager.Instance.mainCamera.aspect) + 1;
        }
        return (Vector2)Character.Instance.transform.position + UnityEngine.Random.insideUnitCircle.normalized * howFar;
    }


}
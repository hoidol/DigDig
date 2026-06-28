using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using UnityEngine;

public class EnemyManager : MonoSingleton<EnemyManager>
{
    readonly Dictionary<EnemyType, Stack<Enemy>> pool = new(); // 적 종류 별 풀링
    readonly HashSet<Enemy> activeEnemies = new();
    static readonly Dictionary<EnemyType, EnemyData> enemyDataDic = new(); //적 종류 별 게임 데이터

    [field: SerializeField] public int ActiveEnemyCount { get; private set; }

    void Awake()
    {
        EnemyData[] enemyDatas = Resources.LoadAll<EnemyData>("EnemyData");
        foreach (EnemyData enemyData in enemyDatas)
            enemyDataDic[enemyData.type] = enemyData;
        GameEventBus.Subscribe<EnemyDeadEvent>(EnemyDeadEventListener);
    }

    public static EnemyData GetEnemyData(EnemyType type)
    {
        return enemyDataDic[type];
    }

    public Enemy Instantiate(Enemy prefab)
    {
        EnemyData data = enemyDataDic[prefab.enemyType];

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

    void EnemyDeadEventListener(EnemyDeadEvent e)
    {
        ReleaseEnemy(e.enemy);
    }

    public Enemy GetEnemyInTileIndex(Vector2Int tileIdx)
    {
        foreach (Enemy enemy in activeEnemies)
        {
            foreach (Vector2Int idx in enemy.tileIndexArr)
            {
                if (idx == tileIdx) return enemy;
            }
        }
        return null;
    }


}
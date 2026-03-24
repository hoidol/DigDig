using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoSingleton<EnemyManager>
{
    readonly Dictionary<EnemyType, Stack<Enemy>> pool = new(); // 적 종류 별 풀링
    readonly Dictionary<EnemyType, EnemyData> enemyDataDic = new(); //적 종류 별 게임 데이터

    void Awake()
    {
        EnemyData[] enemyDatas = Resources.LoadAll<EnemyData>("EnemyData");
        foreach (EnemyData enemyData in enemyDatas)
            enemyDataDic[enemyData.type] = enemyData;

        GameEventBus.Subscribe<EnemyDeadEvent>(EnemyDeadEventListener);
    }

    public Enemy GetEnemy(EnemyType enemyType)
    {
        EnemyData data = enemyDataDic[enemyType];

        if (!pool.ContainsKey(enemyType))
            pool[enemyType] = new Stack<Enemy>();

        Enemy enemy = pool[enemyType].Count > 0
            ? pool[enemyType].Pop()
            : Instantiate(data.prefab);

        enemy.gameObject.SetActive(true);
        enemy.Init(data);
        return enemy;
    }

    void ReleaseEnemy(Enemy enemy)
    {
        if (!pool.ContainsKey(enemy.enemyType))
            pool[enemy.enemyType] = new Stack<Enemy>();

        enemy.gameObject.SetActive(false);
        pool[enemy.enemyType].Push(enemy);
    }

    void EnemyDeadEventListener(EnemyDeadEvent e)
    {
        ReleaseEnemy(e.enemy);
    }
}

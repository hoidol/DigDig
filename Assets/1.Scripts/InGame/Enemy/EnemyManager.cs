using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using UnityEngine;

public class EnemyManager : MonoSingleton<EnemyManager>
{
    readonly Dictionary<EnemyType, Stack<Enemy>> pool = new(); // 적 종류 별 풀링
    readonly Dictionary<EnemyType, EnemyData> enemyDataDic = new(); //적 종류 별 게임 데이터
    public EnemyPatternData[] enemyPatternData;
    [field: SerializeField] public int ActiveEnemyCount { get; private set; }

    void Awake()
    {
        EnemyData[] enemyDatas = Resources.LoadAll<EnemyData>("EnemyData");
        foreach (EnemyData enemyData in enemyDatas)
            enemyDataDic[enemyData.type] = enemyData;

        enemyPatternData = Resources.LoadAll<EnemyPatternData>("EnemyPatternData");
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
        ActiveEnemyCount++;
        return enemy;
    }

    void ReleaseEnemy(Enemy enemy)
    {
        if (!pool.ContainsKey(enemy.enemyType))
            pool[enemy.enemyType] = new Stack<Enemy>();

        enemy.gameObject.SetActive(false);
        pool[enemy.enemyType].Push(enemy);
        ActiveEnemyCount = Mathf.Max(0, ActiveEnemyCount - 1);
    }

    void EnemyDeadEventListener(EnemyDeadEvent e)
    {
        ReleaseEnemy(e.enemy);

    }

    public EnemyPatternData GetEnemyPattern(EnemyPatternType pType)
    {
        return enemyPatternData.FirstOrDefault(e => e.patternType == pType);
    }


}
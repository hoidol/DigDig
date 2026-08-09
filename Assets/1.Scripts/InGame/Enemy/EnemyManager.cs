using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mono.Cecil;
using UnityEngine;

public class EnemyManager : MonoSingleton<EnemyManager>, ILoadData
{
    public static readonly Dictionary<EnemyType, EnemyData> enemyDataDic = new(); //적 종류 별 게임 데이터

    // [field: SerializeField] public int ActiveEnemyCount { get; private set; }

    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync();
    }

    async UniTask LoadDataAsync()
    {
        await AddressableMgr.LoadAllByLabel<EnemyData>("EnemyData", (dates) =>
       {
           EnemyData[] enemyDatas = dates;
           foreach (EnemyData enemyData in enemyDatas)
               enemyDataDic[enemyData.type] = enemyData;
       });
    }

    public static EnemyData GetEnemyData(EnemyType type)
    {
        return enemyDataDic[type];
    }




}
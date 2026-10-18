
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyManager : MonoSingleton<EnemyManager>, ILoadData
{
    public static readonly Dictionary<(EnemyType, DefenceType), EnemyData> enemyDataDic = new(); //적 종류 + 방어 타입 별 게임 데이터

    // [field: SerializeField] public int ActiveEnemyCount { get; private set; }

    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        LoadTask = LoadDataAsync().Preserve();
    }

    async UniTask LoadDataAsync()
    {
        await AddressableMgr.LoadAllByLabel<EnemyData>("EnemyData", (dates) =>
       {
           EnemyData[] enemyDatas = dates;
           foreach (EnemyData enemyData in enemyDatas)
               enemyDataDic[(enemyData.type, enemyData.defenceType)] = enemyData;
       });
    }

    public static EnemyData GetEnemyData(EnemyType type, DefenceType defenceType)
    {
        return enemyDataDic[(type, defenceType)];
    }




}
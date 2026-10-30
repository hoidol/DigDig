
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

    // 해당 방어 타입 데이터가 없으면 같은 종류의 Normal 방어 타입 데이터로 대체 (엘리트는 방어 타입별 데이터 없이 Normal 하나만 사용)
    public static EnemyData GetEnemyData(EnemyType type, DefenceType defenceType)
    {
        if (enemyDataDic.TryGetValue((type, defenceType), out EnemyData data))
            return data;
        if (enemyDataDic.TryGetValue((type, DefenceType.Normal), out data))
            return data;
        Debug.LogWarning($"[EnemyManager] EnemyData 없음 ({type}, {defenceType})");
        return null;
    }




}
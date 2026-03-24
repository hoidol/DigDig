using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "StageData", order = 0)]
public class StageData : ScriptableObject
{
    public string key;
    public int level;
    public UndergroundData[] undergroundDatas;
#if UNITY_EDITOR
    public void Edit()
    {
        for (int i = 0; i < undergroundDatas.Length; i++)
        {
            undergroundDatas[i].idx = i;
            for (int j = 0; j < undergroundDatas[i].waveDatas.Length; j++)
            {
                undergroundDatas[i].waveDatas[j].idx = j;
            }
        }
    }
#endif

}

[System.Serializable]
public class UndergroundData
{
    public int idx;
    public float waveWaitTime;
    public WaveData[] waveDatas;
}

[System.Serializable]
public class WaveData
{
    public int idx;
    public float waveTime;
    public MerchantGrade merchantGrade;
    public EnemySpawnData[] enemySpawnDatas;

}
[System.Serializable]
public class EnemySpawnData
{
    public EnemyType enemyType;
    public string enemyKey;
    public Vector2 intervalRange;
    public Vector2Int countRange;
    public int maxCount; //이거 이상으로 더 안들어남 - 많이 잡아야지 이득!
}
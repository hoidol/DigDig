using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
public class StatManager : MonoSingleton<StatManager>
{
    private Dictionary<string, StatData> statDataDic = new Dictionary<string, StatData>();

    public void Awake()
    {
        var dataArray = Resources.LoadAll<StatData>("StatData");
        foreach (var data in dataArray)
        {
            statDataDic.Add(data.statType.ToString(), data);
        }
    }
    float minDistance;
    void Start()
    {
        GameEventBus.Subscribe<StartGameEvent>(OnStartGame);
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnOreStoneDestroyed);

        SpawnStatSpawn(MapManager.MIN_RANGE_RADIUS + OreStone.SIZE * 2, true);
        SpawnStatSpawn(MapManager.MIN_RANGE_RADIUS + OreStone.SIZE * 5, true);
        SpawnStatSpawn(MapManager.MIN_RANGE_RADIUS + OreStone.SIZE * 8, true);
    }


    List<StatStone> statStones = new();
    int spawnCount;
    int destroyStoneCount;
    //float maxStatStoneDistance = 7;
    //float minStatStoneDistance = 7;
    Vector2 randomGap = new Vector2(1f, 2.5f);
    private void OnStartGame(StartGameEvent e)
    {
        Debug.Log("StatManager OnStartGame");
    }
    float spawnTime = 15;
    float timer = 0;
    void Update()
    {
        if (timer > 0)
            timer -= Time.deltaTime;

        if (timer <= 0)
        {
            SpawnStatSpawn();
        }
    }
    private void OnOreStoneDestroyed(DestroyedStoneEvent e)
    {
        destroyStoneCount++;
        SpawnStatSpawn();
    }

    void SpawnStatSpawn(float far = 0, bool immediate = false)
    {
        if (!immediate)
        {
            if (timer > 0)
                return;

            if (destroyStoneCount < 20)
                return;

            if (statStones.Count >= 4)
                return;
        }



        if (far <= 0)
            far = minDistance + Random.Range(randomGap.x, randomGap.y);

        Vector2 spawnPosition = Random.insideUnitCircle * far;
        for (int i = 0; i < statStones.Count; i++)
        {
            if (statStones[i] != null)
            {
                float dist = Vector2.Distance(statStones[i].transform.position, spawnPosition);
                if (dist < 4)
                {
                    // 너무 가까우면 스폰 위치 재설정
                    spawnPosition = Random.insideUnitCircle * far;
                    i = -1; // 처음부터 다시 체크
                }
            }
        }

        minDistance = far;
        StatStone statStone = StatStone.Get();
        statStones.Add(statStone);
        int lv = 1;
        if (Random.Range(0f, 100) < 20)
            lv = 2;


        List<StatData> statDatas = statDataDic.Values.Where(data => data.Unlocked()).ToList();
        StatData statData = statDatas[Random.Range(0, statDatas.Count)];
        statStone.Spawn(spawnPosition, statData, lv);


        timer = spawnTime;
        destroyStoneCount = 0;
        spawnCount++;
    }

    public StatData GetStatData(string key)
    {
        if (statDataDic.TryGetValue(key, out var data))
        {
            return data;
        }
        else
        {
            Debug.LogError($"StatManager: No StatData found for key {key}");
            return null;
        }
    }

    class StatSpawnCount
    {
        public StatType statType;
        public int count;
    }
}
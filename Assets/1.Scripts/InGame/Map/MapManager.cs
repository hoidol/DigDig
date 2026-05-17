using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MapManager : MonoSingleton<MapManager>
{
    //[SerializeField] private MapDisplay mapDisplay;
    public OreStone oreStonePrefab;

    // 이 거리에서 후순위 색상의 가중치가 크게 올라감
    //[SerializeField] private float referenceDistance = 20f;

    public Color[] fillColors;
    // 각 색상별로 거리(x축) → 가중치(y축) 커브를 Inspector에서 그래프로 설정
    public float[] fixWeights;
    public AnimationCurve[] weightCurves;
    [SerializeField] private float[] weights;
    readonly List<OreStone> activeOres = new();

    public void SpawnMap()
    {
        weights = new float[weightCurves.Length];
        SpawnTile(Vector2.zero, 36, MIN_RANGE_RADIUS);
    }
    void Start()
    {
        GameEventBus.Subscribe<OrdealEndEvent>(OnOrdealEndEvent);

    }

    public const float MIN_RANGE_RADIUS = 6f;
    void OnOrdealEndEvent(OrdealEndEvent e)
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            foreach (var ore in activeOres)
                if (ore != null) ore.Return();
            activeOres.Clear();
            SpawnTile(Player.Instance.transform.position, 36, MIN_RANGE_RADIUS);
        }
    }
    public void SpawnTile(Vector2 pos, float radius, float exclueRadius)
    {
        int snappedX = Mathf.RoundToInt(pos.x / OreStone.SIZE);
        int snappedY = Mathf.RoundToInt(pos.y / OreStone.SIZE);
        int cellRadius = Mathf.CeilToInt(radius / OreStone.SIZE);
        Vector2 snappedPos = new(snappedX * OreStone.SIZE, snappedY * OreStone.SIZE);

        var spawnList = new List<(OreStone ore, float dist)>();

        for (int cx = snappedX - cellRadius; cx <= snappedX + cellRadius; cx++)
        {
            for (int cy = snappedY - cellRadius; cy <= snappedY + cellRadius; cy++)
            {
                Vector2 cellPos = new(cx * OreStone.SIZE, cy * OreStone.SIZE);
                float dist = Vector2.Distance(snappedPos, cellPos);
                if (dist > radius || dist < exclueRadius) continue;

                int colorIdx = PickColorIndex(dist);
                OreStone ore = OreStone.Get(oreStonePrefab, cellPos, transform);
                ore.Init(colorIdx, fillColors[colorIdx]);
                //ore.gameObject.SetActive(false);
                activeOres.Add(ore);
                spawnList.Add((ore, dist));
            }
        }

        // RevealTiles(spawnList).Forget();
    }

    async UniTaskVoid RevealTiles(List<(OreStone ore, float dist)> spawnList)
    {
        spawnList.Sort((a, b) => a.dist.CompareTo(b.dist));

        var token = this.GetCancellationTokenOnDestroy();
        int i = 0;
        while (i < spawnList.Count)
        {
            float curDist = spawnList[i].dist;
            while (i < spawnList.Count && Mathf.Approximately(spawnList[i].dist, curDist) ||
                   i < spawnList.Count && spawnList[i].dist - curDist < OreStone.SIZE * 0.5f)
            {
                if (spawnList[i].ore != null)
                    spawnList[i].ore.gameObject.SetActive(true);
                i++;
            }
            await UniTask.Delay(25, cancellationToken: token);
        }
    }




    // fixWeights[i]: 거리 무관 고정 가중치
    // weightCurves[i]: x=0~1 (distance/100), y=추가 가중치
    private int PickColorIndex(float distance)
    {
        float rate = Mathf.Clamp01(distance / 100f);
        int n = weightCurves.Length;
        float total = 0f;
        //Debug.Log($"distance : {distance}, rate {rate}");
        for (int i = 0; i < n; i++)
        {
            //Debug.Log($"i : {i}, weightCurves[i].Evaluate(rate) {weightCurves[i].Evaluate(rate)}");
            weights[i] = Mathf.Max(0f, weightCurves[i].Evaluate(rate));
            total += weights[i];
        }

        float rand = Random.Range(0f, total);
        float cumulative = 0f;
        for (int i = 0; i < n; i++)
        {
            cumulative += weights[i];
            if (rand < cumulative) return i;
        }
        return n - 1;
    }


    // NPC 등장 시 호출 - 반경 내 타일 제거 및 재생성 방지
    public void ClearTilesInRadius(Vector2 center, float radius)
    {
        // 현재 로드된 타일 즉시 제거
        Collider2D[] cols = Physics2D.OverlapCircleAll(center, radius, LayerMask.GetMask("Hittable"));
        foreach (var col in cols)
        {
            if (col.tag == "OreStone")
            {
                col.GetComponent<OreStone>()?.Destroyed(false);
            }

        }

    }
}

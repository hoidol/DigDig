using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MapManager : MonoSingleton<MapManager>
{
    public static List<Vector2Int> emptyidxs = new List<Vector2Int>();
    public OreStone oreStonePrefab;
    public Color[] fillColors;
    // 각 색상별로 거리(x축) → 가중치(y축) 커브를 Inspector에서 그래프로 설정
    public float[] fixWeights;
    public AnimationCurve[] weightCurves;
    [SerializeField] private float[] weights;
    readonly List<OreStone> activeOres = new();

    public void SpawnMap()
    {
        weights = new float[weightCurves.Length];
        SpawnTile(Vector2.zero, MAX_RANGE_RADIUS, MIN_RANGE_RADIUS);
    }

    public const float MIN_RANGE_RADIUS = 6f;
    public const float MAX_RANGE_RADIUS = 40f;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            foreach (var ore in activeOres)
                if (ore != null) ore.Return();
            activeOres.Clear();
            SpawnTile(Player.Instance.transform.position, MAX_RANGE_RADIUS, MIN_RANGE_RADIUS);
        }
    }
    // List<Vector2Int> tempEmptyIndies = new List<Vector2Int>();
    // List<Vector2Int> GetEmptyIndies(GridPattern gridPattern, bool random) //높은 확률로 기존 타일이 있으면 최대한 붙여서 소환하자
    // {
    //     tempEmptyIndies.Clear();
    //     var sorted = Player.Instance.tileCheckers.OrderBy(c => c.TileCount()).ToList();
    //     // for (int rank = 0; rank < sorted.Count; rank++)
    //     //     Debug.Log($"[{rank}] {sorted[rank].gameObject.name} | checkCount: {sorted[rank].TileCount()} | pos: {sorted[rank].transform.position}");

    //     var bestChecker = sorted.Take(2).OrderBy(_ => Random.value).First();
    //     Debug.Log($"bestChecker {bestChecker.name} | gameObject: {bestChecker.gameObject.name} | pos: {bestChecker.transform.position} | active: {bestChecker.gameObject.activeSelf}");
    //     Vector2 center = bestChecker.transform.position;
    //     Vector2 randomPoint = center + Random.insideUnitCircle * 5f;
    //     Vector2Int startIdx = PositionToIndex(randomPoint);


    //     Vector2 offset = center - (Vector2)Player.Instance.transform.position;
    //     Vector2Int dir = Mathf.Abs(offset.x) > Mathf.Abs(offset.y) ? Vector2Int.up : Vector2Int.right;
    //     Vector2Int otherDir = dir == Vector2Int.up ? Vector2Int.right : Vector2Int.up;
    //     for (int y = 0; y < 2; y++)
    //     {
    //         Vector2Int sIdx = startIdx + otherDir * y;
    //         for (int x = 0; x < 3; x++)
    //         {
    //             Vector2Int idx = sIdx + dir * x;
    //             if (!emptyidxs.Contains(idx))
    //                 continue;
    //             tempEmptyIndies.Add(idx);
    //         }
    //     }
    //     return tempEmptyIndies;
    // }

    //특정 위치
    public static Vector2Int[] GetIndies(Vector2 pos, Vector2 dir, Vector2 farRange, float radius, Vector2Int size)
    {
        Vector2 center = pos + dir.normalized * ((farRange.x + farRange.y) * 0.5f);
        Vector2Int centerIdx = PositionToIndex(center);
        int cellRadius = Mathf.CeilToInt(radius / OreStone.SIZE);

        var result = new List<Vector2Int>();
        for (int cx = centerIdx.x - cellRadius; cx <= centerIdx.x + cellRadius; cx += size.x)
        {
            for (int cy = centerIdx.y - cellRadius; cy <= centerIdx.y + cellRadius; cy += size.y)
            {
                Vector2Int idx = new(cx, cy);
                float distFromCenter = Vector2.Distance(center, IndexToPosition(idx));
                float distFromPos = Vector2.Distance(pos, IndexToPosition(idx));
                if (distFromCenter <= radius && distFromPos >= farRange.x && distFromPos <= farRange.y)
                    result.Add(idx);
            }
        }
        return result.ToArray();
    }

    public static Vector2 SnappedPosition(Vector2 pos)
    {
        int snappedX = Mathf.RoundToInt(pos.x / OreStone.SIZE);
        int snappedY = Mathf.RoundToInt(pos.y / OreStone.SIZE);

        return new(snappedX * OreStone.SIZE, snappedY * OreStone.SIZE);
    }

    public static Vector2Int PositionToIndex(Vector2 pos)
    {
        int x = Mathf.RoundToInt(pos.x / OreStone.SIZE);
        int y = Mathf.RoundToInt(pos.y / OreStone.SIZE);
        return new Vector2Int(x, y);
    }

    public static Vector2 IndexToPosition(Vector2Int idx)
    {
        return new Vector2(idx.x * OreStone.SIZE, idx.y * OreStone.SIZE);
    }

    public static List<Vector2Int> GetIndicesInRadius(Vector2 pos, Vector2 dir, float farRange, float radius)
    {
        Vector2 center = pos + dir.normalized * farRange;
        Vector2Int centerIdx = PositionToIndex(center);
        int cellRadius = Mathf.CeilToInt(radius / OreStone.SIZE);

        var result = new List<Vector2Int>();
        for (int cx = centerIdx.x - cellRadius; cx <= centerIdx.x + cellRadius; cx++)
        {
            for (int cy = centerIdx.y - cellRadius; cy <= centerIdx.y + cellRadius; cy++)
            {
                Vector2 cellPos = IndexToPosition(new Vector2Int(cx, cy));
                if (Vector2.Distance(center, cellPos) <= radius)
                    result.Add(new Vector2Int(cx, cy));
            }
        }
        return result;
    }

    public void RegisterEmpty(List<Vector2Int> indices)
    {
        foreach (var idx in indices)
            if (!emptyidxs.Contains(idx))
                emptyidxs.Add(idx);
    }
    public void RegisterEmpty(Vector2Int index)
    {
        if (!emptyidxs.Contains(index))
            emptyidxs.Add(index);
    }

    public static bool CheckEmpty(Vector2Int index)
    {
        return emptyidxs.Contains(index);
    }


    public void SpawnTile(Vector2 pos, float radius, float exclueRadius)
    {
        int snappedX = Mathf.RoundToInt(pos.x / OreStone.SIZE);
        int snappedY = Mathf.RoundToInt(pos.y / OreStone.SIZE);
        Vector2 snappedPos = SnappedPosition(pos);

        int cellRadius = Mathf.CeilToInt(radius / OreStone.SIZE);

        var spawnList = new List<(OreStone ore, float dist)>();

        for (int cx = snappedX - cellRadius; cx <= snappedX + cellRadius; cx++)
        {
            for (int cy = snappedY - cellRadius; cy <= snappedY + cellRadius; cy++)
            {
                Vector2 cellPos = new(cx * OreStone.SIZE, cy * OreStone.SIZE);
                float dist = Vector2.Distance(snappedPos, cellPos);
                if (dist > radius) continue;

                if (dist < exclueRadius)
                {
                    var emptyIdx = new Vector2Int(cx, cy);
                    RegisterEmpty(emptyIdx);
                    continue;
                }

                int colorIdx = PickColorIndex(dist);
                OreStone ore = OreStone.Get(oreStonePrefab, cellPos, transform);
                ore.Init(colorIdx, fillColors[colorIdx], new Vector2Int(cx, cy));
                //ore.gameObject.SetActive(false);
                activeOres.Add(ore);
                spawnList.Add((ore, dist));
            }
        }
    }

    // async UniTaskVoid RevealTiles(List<(OreStone ore, float dist)> spawnList)
    // {
    //     spawnList.Sort((a, b) => a.dist.CompareTo(b.dist));

    //     var token = this.GetCancellationTokenOnDestroy();
    //     int i = 0;
    //     while (i < spawnList.Count)
    //     {
    //         float curDist = spawnList[i].dist;
    //         while (i < spawnList.Count && Mathf.Approximately(spawnList[i].dist, curDist) ||
    //                i < spawnList.Count && spawnList[i].dist - curDist < OreStone.SIZE * 0.5f)
    //         {
    //             if (spawnList[i].ore != null)
    //                 spawnList[i].ore.gameObject.SetActive(true);
    //             i++;
    //         }
    //         await UniTask.Delay(25, cancellationToken: token);
    //     }
    // }

    public static Vector2 GetCenterPostion(List<Vector2Int> indexs)
    {
        Vector2 sum = Vector2.zero;
        for (int i = 0; i < indexs.Count; i++)
        {
            sum += IndexToPosition(indexs[i]);
        }
        return sum / indexs.Count;
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
    public void ClearTilesInRadius(Vector2 center, float width, float height)
    {
        // 현재 로드된 타일 즉시 제거
        var mask = LayerMask.GetMask("Hittable");
        Collider2D[] cols = Mathf.Approximately(width, height)
            ? Physics2D.OverlapCircleAll(center, width / 2f, mask)
            : Physics2D.OverlapBoxAll(center, new Vector2(width, height), 0f, mask);
        foreach (var col in cols)
        {
            if (col.tag == "OreStone")
            {
                col.GetComponent<OreStone>()?.Destroyed(false);
            }

        }

    }
}

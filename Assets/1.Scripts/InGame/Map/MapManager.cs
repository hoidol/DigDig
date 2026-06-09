using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MapManager : MonoSingleton<MapManager>
{

    public const float TILE_SIZE = 1.46f;
    //public static List<Vector2Int> emptyIndexs = new List<Vector2Int>();
    public static bool[,] emptyTileArray;
    public static Vector2[,] tilePositionArray;
    public OreStone oreStonePrefab;
    public Color[] fillColors;
    // 각 색상별로 거리(x축) → 가중치(y축) 커브를 Inspector에서 그래프로 설정
    public float[] fixWeights;
    public AnimationCurve[] weightCurves;
    [SerializeField] private float[] weights;

    public void SpawnMap()
    {
        weights = new float[weightCurves.Length];
        emptyTileArray = new bool[MAX_RANGE_RADIUS * 2, MAX_RANGE_RADIUS * 2];
        tilePositionArray = new Vector2[MAX_RANGE_RADIUS * 2, MAX_RANGE_RADIUS * 2];
        SpawnTile( MAX_RANGE_RADIUS, MIN_RANGE_RADIUS);
    }

    public const int MIN_RANGE_RADIUS = 6;
    public const int MAX_RANGE_RADIUS = 40;


    
    //특정 위치
    public static Vector2Int[] GetIndies(Vector2 pos, Vector2 dir, Vector2 farRange, float radius, Vector2Int size)
    {
        Vector2 center = pos + dir.normalized * ((farRange.x + farRange.y) * 0.5f);
        Vector2Int centerIdx = PositionToIndex(center);
        int cellRadius = Mathf.CeilToInt(radius / TILE_SIZE);

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
        int snappedX = Mathf.RoundToInt(pos.x / TILE_SIZE);
        int snappedY = Mathf.RoundToInt(pos.y / TILE_SIZE);

        return new(snappedX * TILE_SIZE, snappedY * TILE_SIZE);
    }

    public static Vector2Int PositionToIndex(Vector2 pos)
    {
        int x = Mathf.RoundToInt(pos.x / TILE_SIZE);
        int y = Mathf.RoundToInt(pos.y / TILE_SIZE);
        return new Vector2Int(x, y);
    }

    public static Vector2 IndexToPosition(Vector2Int idx)
    {
        return new Vector2(idx.x * TILE_SIZE, idx.y * TILE_SIZE);
    }


    public static Vector2 IndexToPosition(Vector2Int[,] idxArr)
    {
        Vector2 sum = Vector2.zero;
        foreach(Vector2Int idx in idxArr)
        {
            sum += tilePositionArray[idx.x,idx.y];
        }
        Vector2 center = sum / (idxArr.GetLength(0) * idxArr.GetLength(1));
        return center;
    }


    public static List<Vector2Int> GetIndicesInRadius(Vector2 pos, Vector2 dir, float farRange, float radius)
    {
        Vector2 center = pos + dir.normalized * farRange;
        Vector2Int centerIdx = PositionToIndex(center);
        int cellRadius = Mathf.CeilToInt(radius / TILE_SIZE);

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

    public static void ReleaseTile(Vector2Int[,] indexArr)
    {
        foreach (var index in indexArr)
            ReleaseTile(index);
    }

    public static void ReleaseTile(Vector2Int index)
    {
        emptyTileArray[index.x + MAX_RANGE_RADIUS, index.y + MAX_RANGE_RADIUS] = true;
    }
    public static void RegisterTile(Vector2Int[,] indexArr)
    {
        foreach (var index in indexArr)
            RegisterTile(index);
    }
    public static void RegisterTile(Vector2Int index)
    {
        emptyTileArray[index.x + MAX_RANGE_RADIUS, index.y + MAX_RANGE_RADIUS] = false;
    }

    public static bool CheckEmpty(Vector2Int index)
    {
        return emptyTileArray[index.x, index.y];
    }


    public void SpawnTile(float radius, float exclueRadius)
    {
        // int snappedX = Mathf.RoundToInt(pos.x / TILE_SIZE);
        // int snappedY = Mathf.RoundToInt(pos.y / TILE_SIZE);
        // Vector2 snappedPos = SnappedPosition(pos);

        int cellRadius = Mathf.CeilToInt(radius / TILE_SIZE);

        var spawnList = new List<(OreStone ore, float dist)>();

        for (int cx =  -cellRadius; cx <= cellRadius; cx++)
        {
            for (int cy = -cellRadius; cy <= cellRadius; cy++)
            {   
                Vector2Int index = new Vector2Int(MAX_RANGE_RADIUS+cx, MAX_RANGE_RADIUS+cy); 
                Vector2 cellPos = new(cx * TILE_SIZE, cy * TILE_SIZE);
                tilePositionArray[index.x, index.y] = cellPos;

                float dist = Vector2.Distance(Vector2.zero, cellPos);
                if (dist > radius) continue;

                if (dist < exclueRadius)
                {
                    var emptyIdx = new Vector2Int(cx, cy);
                    ReleaseTile(emptyIdx);
                    continue;
                }

                int colorIdx = PickColorIndex(dist);
                OreStone ore = OreStone.Get(oreStonePrefab, cellPos, transform);
                Vector2Int[,] indexArr = new Vector2Int[1,1];
                indexArr[0,0] = index;
                ore.Init(colorIdx, fillColors[colorIdx], indexArr);
   
                spawnList.Add((ore, dist));
            }
        }
    }


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
    //이동 가능한지 체크
    public static bool CheckMoveTo(Vector2Int[,] idxArr, Vector2Int dir) //dir방향으로 갈 수 있는지 확인
    {
        var currentSet = new HashSet<Vector2Int>();
        foreach (var idx in idxArr)
            currentSet.Add(idx);

        foreach (var idx in idxArr)
        {
            Vector2Int next = idx + dir;
            if (currentSet.Contains(next)) continue; // 자신이 이미 점유 중인 타일은 통과

            int ax = next.x + MAX_RANGE_RADIUS;
            int ay = next.y + MAX_RANGE_RADIUS;
            if (ax < 0 || ay < 0 ||
                ax >= emptyTileArray.GetLength(0) || ay >= emptyTileArray.GetLength(1))
                return false;

            if (!emptyTileArray[ax, ay])
                return false;
        }
        return true;
    }


    public static Vector2Int[,] GetIndexArray(Vector2Int[,] idxArr, Vector2Int dir) //dir방향으로 갈 수 있는지 확인
    {
        Vector2Int[,] array = new Vector2Int[idxArr.GetLength(0),idxArr.GetLength(1)];
        var currentSet = new HashSet<Vector2Int>();
        foreach (var idx in idxArr)
            currentSet.Add(idx);

        for (int x =0;x<idxArr.GetLength(0);x++)
        {
            for (int y = 0; y < idxArr.GetLength(1); y++)
            {
                array[x,y] = idxArr[x,y] + dir;
            }
        }
        return array;
    }
}

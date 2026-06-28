using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MapManager : MonoSingleton<MapManager>
{

    public const float TILE_SIZE = 1.46f;
    //public static List<Vector2Int> emptyIndexs = new List<Vector2Int>();
    public static ITile[,] tileArray;
    public static Vector2[,] tilePositionArray;
    // public OreStone oreStonePrefab;
    public Color[] fillColors;
    // 각 색상별로 거리(x축) → 가중치(y축) 커브를 Inspector에서 그래프로 설정
    public float[] fixWeights;
    public AnimationCurve[] weightCurves;
    [SerializeField] private float[] weights;

    public static List<Vector2Int> GetEmptyTileIndexesInRange(Vector2 playerPos, int includeSize, int excludeSize = 0)
    {
        List<Vector2Int> indies = new();
        Vector2Int centerIdx = PositionToTileIndex(playerPos);

        for (int x = -includeSize; x <= includeSize; x++)
        {
            for (int y = -includeSize; y <= includeSize; y++)
            {
                if (Mathf.Abs(x) <= excludeSize && Mathf.Abs(y) <= excludeSize)
                    continue;
                Vector2Int idx = centerIdx + new Vector2Int(x, y);
                if (idx.x < 0 || idx.y < 0 || idx.x >= tileArray.GetLength(0) || idx.y >= tileArray.GetLength(1))
                    continue;
                if (CheckEmpty(idx))
                    indies.Add(idx);
            }
        }
        return indies;
    }

    public void SpawnMap()
    {

        weights = new float[weightCurves.Length];
        tileArray = new ITile[MAX_RANGE_RADIUS * 2, MAX_RANGE_RADIUS * 2];
        tilePositionArray = new Vector2[MAX_RANGE_RADIUS * 2, MAX_RANGE_RADIUS * 2];
        SpawnTile(MAX_RANGE_RADIUS, MIN_RANGE_RADIUS);
    }

    public const int MIN_RANGE_RADIUS = 5;
    public const int MAX_RANGE_RADIUS = 40;


    public static Vector2 SnappedPosition(Vector2 pos)
    {
        int snappedX = Mathf.RoundToInt(pos.x / TILE_SIZE);
        int snappedY = Mathf.RoundToInt(pos.y / TILE_SIZE);

        return new(snappedX * TILE_SIZE, snappedY * TILE_SIZE);
    }


    public static Vector2 TileIndexToPosition(Vector2Int idx)
    {
        Vector2Int originIndex = new Vector2Int(idx.x - MAX_RANGE_RADIUS, idx.y - MAX_RANGE_RADIUS);
        return new Vector2(originIndex.x * TILE_SIZE, originIndex.y * TILE_SIZE);
    }

    public static Vector2Int PositionToTileIndex(Vector2 pos)
    {
        Vector2 spappedPos = SnappedPosition(pos);


        int x = Mathf.RoundToInt(spappedPos.x / TILE_SIZE) + MAX_RANGE_RADIUS;
        int y = Mathf.RoundToInt(spappedPos.y / TILE_SIZE) + MAX_RANGE_RADIUS;
        return new Vector2Int(x, y);
    }

    public static Vector2 TileIndexToCenterPosition(Vector2Int[,] idxArr)
    {
        Vector2 sum = Vector2.zero;
        foreach (Vector2Int idx in idxArr)
        {
            sum += tilePositionArray[idx.x, idx.y];
        }
        Vector2 center = sum / (idxArr.GetLength(0) * idxArr.GetLength(1));
        return center;
    }


    public static void ReleaseTile(Vector2Int[,] indexArr)
    {
        foreach (var index in indexArr)
            ReleaseTile(index);
    }

    public static void ReleaseTile(Vector2Int index)
    {
        // Debug.Log($"MapMgr ReleaseTile x {index.x + MAX_RANGE_RADIUS} y {index.y + MAX_RANGE_RADIUS}");
        tileArray[index.x, index.y] = null;
    }
    public static void RegisterTile(Vector2Int[,] indexArr, ITile tile)
    {
        foreach (var index in indexArr)
            RegisterTile(index, tile);
    }
    public static void RegisterTile(Vector2Int index, ITile tile)
    {
        tileArray[index.x, index.y] = tile;
    }

    public static bool CheckEmpty(Vector2Int index)
    {
        return tileArray[index.x, index.y] == null;
    }


    public void SpawnTile(float radius, float exclueRadius)
    {
        // int snappedX = Mathf.RoundToInt(pos.x / TILE_SIZE);
        // int snappedY = Mathf.RoundToInt(pos.y / TILE_SIZE);
        // Vector2 snappedPos = SnappedPosition(pos);

        int cellRadius = Mathf.CeilToInt(radius / TILE_SIZE);

        var spawnList = new List<(OreStone ore, float dist)>();

        for (int cx = -cellRadius; cx <= cellRadius; cx++)
        {
            for (int cy = -cellRadius; cy <= cellRadius; cy++)
            {
                Vector2Int index = new Vector2Int(MAX_RANGE_RADIUS + cx, MAX_RANGE_RADIUS + cy);
                Vector2 cellPos = new(cx * TILE_SIZE, cy * TILE_SIZE);
                tilePositionArray[index.x, index.y] = cellPos;

                float dist = Vector2.Distance(Vector2.zero, cellPos);
                if (dist > radius) continue;

                if (dist < exclueRadius)
                {
                    // var emptyIdx = new Vector2Int(cx, cy);
                    ReleaseTile(index);
                    continue;
                }

                int colorIdx = PickColorIndex(dist);
                OreStone ore = OreStone.Get(cellPos, transform);
                Vector2Int[,] indexArr = new Vector2Int[1, 1];
                indexArr[0, 0] = index;
                ore.Init(colorIdx, fillColors[colorIdx], indexArr);

                spawnList.Add((ore, dist));
            }
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

            int ax = next.x;
            int ay = next.y;
            if (ax < 0 || ay < 0 ||
                ax >= tileArray.GetLength(0) || ay >= tileArray.GetLength(1))
                return false;

            if (!CheckEmpty(new Vector2Int(ax, ay)))
                return false;
        }
        return true;
    }


    public static Vector2Int[,] GetIndexArray(Vector2Int[,] idxArr, Vector2Int dir) //dir방향으로 갈 수 있는지 확인
    {
        Vector2Int[,] array = new Vector2Int[idxArr.GetLength(0), idxArr.GetLength(1)];
        var currentSet = new HashSet<Vector2Int>();
        foreach (var idx in idxArr)
            currentSet.Add(idx);

        for (int x = 0; x < idxArr.GetLength(0); x++)
        {
            for (int y = 0; y < idxArr.GetLength(1); y++)
            {
                array[x, y] = idxArr[x, y] + dir;
            }
        }
        return array;
    }


    //idxArr를 중심으로 
    public static Vector2Int[,] GetIndexArray(Vector2Int idxArr, Vector2Int size)
    {
        Vector2Int[,] tileIndexs = new Vector2Int[size.x, size.y];
        int startX = Mathf.Clamp(idxArr.x - size.x / 2, 0, MAX_RANGE_RADIUS * 2 - size.x);
        int startY = Mathf.Clamp(idxArr.y - size.y / 2, 0, MAX_RANGE_RADIUS * 2 - size.y);
        for (int x = startX; x < startX + size.x; x++)
        {
            for (int y = startY; y < startY + size.y; y++)
            {
                tileIndexs[x - startX, y - startY] = new Vector2Int(x, y);
            }
        }
        return tileIndexs;
    }

    public static bool GetTileArray(Vector2Int startTileIndex, Vector2Int size, out Vector2Int[,] tileArrays)
    {
        tileArrays = new Vector2Int[size.x, size.y]; // out은 내부에서 할당 필수
        bool empty = true;
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int tileIndex = startTileIndex + new Vector2Int(x, y);
                tileArrays[x, y] = tileIndex;

                if (!CheckEmpty(new Vector2Int(tileIndex.x, tileIndex.y)))
                    empty = false;
            }
        }
        return empty;
    }
    Vector2Int unbreakableCenterTileIndex;
    int unbreakableXCount = 60;
    int unbreakableYCount = 40;
    public List<UnbreakableStone> MakeUnbreakableStone(Vector2Int centerTileIndex, int xCount = 60, int yCount = 40)
    {
        List<UnbreakableStone> list = new List<UnbreakableStone>();
        int leftX = centerTileIndex.x - xCount / 2;
        int bottomY = centerTileIndex.y - yCount / 2;

        unbreakableCenterTileIndex = centerTileIndex;
        unbreakableXCount = xCount;
        unbreakableYCount = yCount;

        for (int x = leftX; x < leftX + xCount; x++)
        {

            for (int y = bottomY; y < bottomY + yCount; y++)
            {
                if ((x == leftX || x == leftX + xCount - 1) || (y == bottomY || y == bottomY + yCount - 1))
                {
                    if (!CheckEmpty(new Vector2Int(x, y)))
                    {
                        //이 자리에있는 것들 제거하기
                        tileArray[x, y].ReleaseTile();
                    }
                    UnbreakableStone unbreakableStone = UnbreakableStone.Get(TileIndexToPosition(new Vector2Int(x, y)), transform);
                    list.Add(unbreakableStone);
                }


            }
        }
        return list;
    }
    public bool CheckUnbreakableArea(Vector2Int tileIndex)
    {
        int leftX = unbreakableCenterTileIndex.x - unbreakableXCount / 2;
        int bottomY = unbreakableCenterTileIndex.y - unbreakableYCount / 2;
        return tileIndex.x >= leftX && tileIndex.x < leftX + unbreakableXCount &&
               tileIndex.y >= bottomY && tileIndex.y < bottomY + unbreakableYCount;
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MapManager : MonoSingleton<MapManager>
{

    public const float TILE_SIZE = 1.46f;
    //public static List<Vector2Int> emptyIndexs = new List<Vector2Int>();

    public static HashSet<Vector2Int> usedTileIdxs = new HashSet<Vector2Int>();
    // public OreStone oreStonePrefab;
    public Color[] fillColors;
    // 각 색상별로 거리(x축) → 가중치(y축) 커브를 Inspector에서 그래프로 설정
    public float[] fixWeights;
    public AnimationCurve[] weightCurves;
    [SerializeField] private float[] weights;

    // public static List<Vector2Int> GetEmptyTileIndexesInRange(Vector2 characterPos, int includeSize, int excludeSize = 0)
    // {
    //     List<Vector2Int> indies = new();
    //     Vector2Int centerIdx = PositionToTileIndex(characterPos);

    //     for (int x = -includeSize; x <= includeSize; x++)
    //     {
    //         for (int y = -includeSize; y <= includeSize; y++)
    //         {
    //             if (Mathf.Abs(x) <= excludeSize && Mathf.Abs(y) <= excludeSize)
    //                 continue;
    //             Vector2Int idx = centerIdx + new Vector2Int(x, y);
    //             if (idx.x < 0 || idx.y < 0 || idx.x >= tileArray.GetLength(0) || idx.y >= tileArray.GetLength(1))
    //                 continue;
    //             if (CheckEmpty(idx))
    //                 indies.Add(idx);
    //         }
    //     }
    //     return indies;
    // }

    void Start()
    {
        GameEventBus.Subscribe<StartGameEvent>(OnStartGameEvent);
    }
    void OnStartGameEvent(StartGameEvent e)
    {
        SpawnMap();
    }

    public const int MIN_RANGE_RADIUS = 5;
    public const int MAX_RANGE_RADIUS = 20;

    public void SpawnMap()
    {
        weights = new float[weightCurves.Length];
        SpawnTile(Vector2.zero, MAX_RANGE_RADIUS);
        CheckSpawnMap().Forget();
    }
    async UniTask CheckSpawnMap()
    {
        while (true)
        {
            await UniTask.Delay(2000);
            SpawnTile(Character.Instance.transform.position, MAX_RANGE_RADIUS);
        }
    }
    public static Vector2 SnappedPosition(Vector2 pos)
    {
        int snappedX = Mathf.RoundToInt(pos.x / TILE_SIZE);
        int snappedY = Mathf.RoundToInt(pos.y / TILE_SIZE);

        return new(snappedX * TILE_SIZE, snappedY * TILE_SIZE);
    }

    public static Vector2 TileIndexToPosition(Vector2Int idx)
    {
        return new Vector2(idx.x * TILE_SIZE, idx.y * TILE_SIZE);
    }

    public static Vector2Int PositionToTileIndex(Vector2 pos)
    {
        Vector2 spappedPos = SnappedPosition(pos);

        int x = Mathf.RoundToInt(spappedPos.x / TILE_SIZE);
        int y = Mathf.RoundToInt(spappedPos.y / TILE_SIZE);
        return new Vector2Int(x, y);
    }

    public static Vector2 TileIndexToCenterPosition(Vector2Int[,] idxArr)
    {
        Vector2 sum = Vector2.zero;
        foreach (Vector2Int idx in idxArr)
        {
            sum += TileIndexToPosition(idx);
        }
        Vector2 center = sum / (idxArr.GetLength(0) * idxArr.GetLength(1));
        return center;
    }

    void ReleaseTile(Vector2Int index)
    {
        Collider2D[] collider2Ds = Physics2D.OverlapPointAll(TileIndexToPosition(index), LayerMask.GetMask("Hittable"));
        for (int i = 0; i < collider2Ds.Length; i++)
        {
            if (collider2Ds[i].gameObject.TryGetComponent(out ITile target))
            {
                target.Destroy();
            }
        }
    }

    public void SpawnTile(Vector2 pos, float radius)
    {
        Debug.Log("MapManager SpawnTile");
        float startX = pos.x - radius;
        float startY = pos.y - radius;
        float endX = pos.x + radius;
        float endY = pos.y + radius;

        Debug.Log($"MapManager SpawnTile startX {startX} startY {startY}");
        // Debug.Log($"MapManager SpawnTile startIdx.x {startIdx.x} endIdx.x {endIdx.x}");
        Vector2Int startIdx = PositionToTileIndex(new Vector2(startX, startY));
        Vector2Int endIdx = PositionToTileIndex(new Vector2(endX, endY));

        // Debug.Log($"MapManager SpawnTile startIdx.x {startIdx.x} endIdx.x {endIdx.x}");
        // Debug.Log($"MapManager SpawnTile startIdx.y {startIdx.y} endIdx.y {endIdx.y}");
        for (int x = startIdx.x; x < endIdx.x; x++)
        {
            for (int y = startIdx.y; y < endIdx.y; y++)
            {
                Vector2Int index = new Vector2Int(x, y);
                if (usedTileIdxs.Contains(index))
                {
                    // Debug.Log($"index {index} 이미 사용됨");
                    continue;
                }
                Vector2 cellPos = TileIndexToPosition(index);
                float dist = Vector2.Distance(Vector2.zero, cellPos);
                // Debug.Log($"cellPos {cellPos} 생성될 위치 ");
                if (dist < MIN_RANGE_RADIUS)
                {
                    // Debug.Log($"index {index} cellPos {cellPos} Vector2 zero에 너무 가까움");
                    // ReleaseTile(index);
                    continue;
                }

                usedTileIdxs.Add(index);
                int colorIdx = PickColorIndex(dist);
                Stone ore = Stone.Get(cellPos, transform);
                ore.gameObject.name = $"Stone {index.x} {index.y}";
                Vector2Int[,] indexArr = new Vector2Int[1, 1];
                indexArr[0, 0] = index;
                ore.Init(colorIdx, fillColors[colorIdx], indexArr);

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
            col.GetComponent<Stone>()?.Destroy();
        }
    }



    //이동 가능한지 체크
    // public static bool CheckMoveTo(Vector2Int[,] idxArr, Vector2Int dir) //dir방향으로 갈 수 있는지 확인
    // {
    //     var currentSet = new HashSet<Vector2Int>();
    //     foreach (var idx in idxArr)
    //         currentSet.Add(idx);

    //     foreach (var idx in idxArr)
    //     {
    //         Vector2Int next = idx + dir;
    //         if (currentSet.Contains(next)) continue; // 자신이 이미 점유 중인 타일은 통과

    //         int ax = next.x;
    //         int ay = next.y;
    //         if (ax < 0 || ay < 0 ||
    //             ax >= tileArray.GetLength(0) || ay >= tileArray.GetLength(1))
    //             return false;

    //         if (!CheckEmpty(new Vector2Int(ax, ay)))
    //             return false;
    //     }
    //     return true;
    // }


    // public static Vector2Int[,] GetIndexArray(Vector2Int[,] idxArr, Vector2Int dir) //dir방향으로 갈 수 있는지 확인
    // {
    //     Vector2Int[,] array = new Vector2Int[idxArr.GetLength(0), idxArr.GetLength(1)];
    //     var currentSet = new HashSet<Vector2Int>();
    //     foreach (var idx in idxArr)
    //         currentSet.Add(idx);

    //     for (int x = 0; x < idxArr.GetLength(0); x++)
    //     {
    //         for (int y = 0; y < idxArr.GetLength(1); y++)
    //         {
    //             array[x, y] = idxArr[x, y] + dir;
    //         }
    //     }
    //     return array;
    // }


    //idxArr를 중심으로 
    // public static Vector2Int[,] GetIndexArray(Vector2Int idxArr, Vector2Int size)
    // {
    //     Vector2Int[,] tileIndexs = new Vector2Int[size.x, size.y];
    //     int startX = Mathf.Clamp(idxArr.x - size.x / 2, 0, MAX_RANGE_RADIUS * 2 - size.x);
    //     int startY = Mathf.Clamp(idxArr.y - size.y / 2, 0, MAX_RANGE_RADIUS * 2 - size.y);
    //     for (int x = startX; x < startX + size.x; x++)
    //     {
    //         for (int y = startY; y < startY + size.y; y++)
    //         {
    //             tileIndexs[x - startX, y - startY] = new Vector2Int(x, y);
    //         }
    //     }
    //     return tileIndexs;
    // }

    // public static bool GetTileArray(Vector2Int startTileIndex, Vector2Int size, out Vector2Int[,] tileArrays)
    // {
    //     tileArrays = new Vector2Int[size.x, size.y]; // out은 내부에서 할당 필수
    //     bool empty = true;
    //     for (int x = 0; x < size.x; x++)
    //     {
    //         for (int y = 0; y < size.y; y++)
    //         {
    //             Vector2Int tileIndex = startTileIndex + new Vector2Int(x, y);
    //             tileArrays[x, y] = tileIndex;

    //             if (!CheckEmpty(new Vector2Int(tileIndex.x, tileIndex.y)))
    //                 empty = false;
    //         }
    //     }
    //     return empty;
    // }



    Vector2 centerTilePos;
    int unbreakableXCount = 60;
    int unbreakableYCount = 40;
    public List<UnbreakableStone> MakeUnbreakableStone(Vector2 centerTilePos, int xCount = 60, int yCount = 40)
    {
        List<UnbreakableStone> list = new List<UnbreakableStone>();

        this.centerTilePos = centerTilePos;
        unbreakableXCount = xCount;
        unbreakableYCount = yCount;

        int leftX = -xCount / 2;
        int bottomY = -yCount / 2;

        for (int x = -xCount / 2; x < xCount / 2; x++)
        {
            for (int y = -yCount / 2; y < yCount / 2; y++)
            {
                if (x == leftX || x == leftX + xCount - 1 || y == bottomY || y == bottomY + yCount - 1)
                {
                    float posX = centerTilePos.x + x * TILE_SIZE;
                    float posY = centerTilePos.y + y * TILE_SIZE;


                    Vector2Int index = PositionToTileIndex(new Vector2(posX, posY));
                    ReleaseTile(index);

                    UnbreakableStone unbreakableStone = UnbreakableStone.Get(TileIndexToPosition(new Vector2Int(x, y)), transform);
                    unbreakableStone.Init(0, Color.white, new Vector2Int[,] { { new Vector2Int(x, y) } });
                    list.Add(unbreakableStone);
                }


            }
        }
        return list;
    }
    // public bool CheckUnbreakableArea(Vector2Int tileIndex)
    // {
    //     int leftX = unbreakableCenterTileIndex.x - unbreakableXCount / 2;
    //     int bottomY = unbreakableCenterTileIndex.y - unbreakableYCount / 2;
    //     return tileIndex.x >= leftX && tileIndex.x < leftX + unbreakableXCount &&
    //            tileIndex.y >= bottomY && tileIndex.y < bottomY + unbreakableYCount;
    // }
}

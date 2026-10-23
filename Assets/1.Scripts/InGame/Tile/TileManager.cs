using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoSingleton<TileManager>
{
    public TileGroup[] tileGroups;
    public Tile[] tiles;
    readonly List<Tile> emptyTiles = new();
    List<Tile> usingTiles = new List<Tile>();

    void Awake()
    {
        tileGroups = GetComponentsInChildren<TileGroup>();
        tiles = GetComponentsInChildren<Tile>();
        usingTiles.AddRange(tiles);
        tileLayer = LayerMask.GetMask("Tile");
        AssignTileIndices();
        BlockRandomTiles(GameSetting.BLOCKED_TILE_COUNT);
    }

    // 전체 타일 격자 좌표 (TileGroup 3*3이 2*2로 놓여 전체 6*6)
    readonly Dictionary<Vector2Int, Tile> tileByIndex = new();

    // 위치로 Tile.index 지정: 왼쪽→오른쪽 x 0부터, 맨 윗줄 y 0부터 아래로 증가
    // TileGroup 사이 간격이 달라도 열/행 순서로만 매기므로 그룹 경계를 넘어 이어짐 (2,0) 옆은 (3,0)
    void AssignTileIndices()
    {
        List<float> xs = new List<float>();
        List<float> ys = new List<float>();
        foreach (Tile tile in tiles)
        {
            AddDistinct(xs, tile.transform.position.x);
            AddDistinct(ys, tile.transform.position.y);
        }
        xs.Sort();
        ys.Sort((a, b) => b.CompareTo(a)); // 위쪽(y 큰 값)이 0

        tileByIndex.Clear();
        foreach (Tile tile in tiles)
        {
            tile.index = new Vector2Int(FindIndex(xs, tile.transform.position.x), FindIndex(ys, tile.transform.position.y));
            tile.tileGroup = tile.GetComponentInParent<TileGroup>();
            if (!tileByIndex.TryAdd(tile.index, tile))
                Debug.LogWarning($"TileManager 같은 index 타일 중복 {tile.index} {tile.name}");
        }
    }

    const float INDEX_EPSILON = 0.05f; // 같은 열/행으로 볼 위치 오차
    static void AddDistinct(List<float> list, float v)
    {
        if (FindIndex(list, v) < 0)
            list.Add(v);
    }
    static int FindIndex(List<float> list, float v)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (Mathf.Abs(list[i] - v) < INDEX_EPSILON)
                return i;
        }
        return -1;
    }

    public Tile GetTile(Vector2Int index)
    {
        return tileByIndex.TryGetValue(index, out Tile tile) ? tile : null;
    }

    // center 기준으로 offsets(index 단위) 위치에 있는 사용 가능한(막히지 않은) 타일들
    public List<Tile> GetTilesAround(Tile center, Vector2Int[] offsets)
    {
        List<Tile> result = new List<Tile>();
        if (center == null)
            return result;

        foreach (Vector2Int offset in offsets)
        {
            Tile tile = GetTile(center.index + offset);
            if (tile != null && !tile.isBlocked)
                result.Add(tile);
        }
        return result;
    }

    // 게임 시작 시 랜덤 타일 count개를 사용 불가로 막음 (중복 없이)
    void BlockRandomTiles(int count)
    {
        List<Tile> candidates = new List<Tile>(tiles);
        count = Mathf.Min(count, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, candidates.Count);
            candidates[idx].Block();
            usingTiles.Remove(candidates[idx]);
            candidates.RemoveAt(idx);
        }
    }
    public bool HasBlockedTile => usingTiles.Count < tiles.Length;

    // 막힌 타일 중 랜덤 하나 해제 (개척 타일 아이템). 해제할 게 없으면 false
    public bool UnblockRandomTile()
    {
        List<Tile> blocked = new List<Tile>();
        foreach (Tile tile in tiles)
        {
            if (tile.isBlocked)
                blocked.Add(tile);
        }
        if (blocked.Count == 0)
            return false;

        Tile target = blocked[Random.Range(0, blocked.Count)];
        target.Unblock();
        usingTiles.Add(target);
        return true;
    }

    void Start()
    {

    }
    // 배치 가능한(비어있는) 타일 중 랜덤 하나 반환, 없으면 null
    public Tile GetRandomEmptyTile()
    {
        emptyTiles.Clear();
        foreach (Tile tile in tiles)
        {
            if (tile.IsEmpty)
                emptyTiles.Add(tile);
        }

        if (emptyTiles.Count == 0)
            return null;

        return emptyTiles[Random.Range(0, emptyTiles.Count)];
    }

    // 아이템이 없는 타일 중 랜덤 하나 반환, 없으면 null
    // 위에 슬라임/캐릭터가 있어도 설치 가능 (설치 즉시 효과 적용)
    public Tile GetRandomTileForItem()
    {
        emptyTiles.Clear();
        foreach (Tile tile in usingTiles)
        {
            if (tile.CanAddTileItem)
                emptyTiles.Add(tile);
        }

        if (emptyTiles.Count == 0)
            return null;

        return emptyTiles[Random.Range(0, emptyTiles.Count)];
    }


    LayerMask tileLayer;
    public Tile GetNearestTile(Vector2 pos)
    {
        Collider2D col = Physics2D.OverlapPoint(pos, tileLayer);
        if (col != null && col.TryGetComponent<Tile>(out Tile t))
        {
            return t;
        }
        return null;
    }

    public Tile GetTile(ITileObject tileObject)
    {
        foreach (Tile tile in tiles)
        {
            if (tile.tileObject == tileObject)
                return tile;
        }
        return null;
    }

    public void RemoveTileObject(ITileObject tileObject)
    {
        Tile tile = GetTile(tileObject);
        if (tile != null)
            tile.Clear();
    }
}

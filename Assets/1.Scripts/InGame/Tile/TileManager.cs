using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoSingleton<TileManager>
{
    public TileGroup[] tileGroups;
    public Tile[] tiles;
    readonly List<Tile> emptyTiles = new();

    void Awake()
    {
        tileGroups = GetComponentsInChildren<TileGroup>();
        tiles = GetComponentsInChildren<Tile>();
        tileLayer = LayerMask.GetMask("Tile");
        BlockRandomTiles(GameSetting.BLOCKED_TILE_COUNT);
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
            candidates.RemoveAt(idx);
        }
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

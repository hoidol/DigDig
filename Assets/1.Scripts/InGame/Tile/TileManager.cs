using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoSingleton<TileManager>
{
    public Tile[] tiles;
    readonly List<Tile> emptyTiles = new();

    void Awake()
    {
        tiles = GetComponentsInChildren<Tile>();
        tileLayer = LayerMask.GetMask("Tile");
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


    LayerMask tileLayer ;
    public Tile GetNearestTile(Vector2 pos)
    {
        Collider2D col = Physics2D.OverlapPoint(pos, tileLayer);
        if (col != null && col.TryGetComponent<Tile>(out Tile t))
        {
            return t;
        }
        return null;
    }

    public Tile GetTile(Slime slime)
    {
        foreach (Tile tile in tiles)
        {
            if (tile.slime == slime)
                return tile;
        }
        return null;
    }

    public void RemoveSlime(Slime slime)
    {
        Tile tile = GetTile(slime);
        if (tile != null)
            tile.Clear();
    }
}

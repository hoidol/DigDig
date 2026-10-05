using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoSingleton<TileManager>
{
    public Tile[] tiles;
    readonly List<Tile> emptyTiles = new();

    void Awake()
    {
        tiles = GetComponentsInChildren<Tile>();
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

    // pos에서 maxDistance 이내의 가장 가까운 타일 반환, 없으면 null
    public Tile GetNearestTile(Vector2 pos, float maxDistance)
    {
        Tile nearest = null;
        float minDist = maxDistance * maxDistance;
        foreach (Tile tile in tiles)
        {
            float dist = ((Vector2)tile.transform.position - pos).sqrMagnitude;
            if (dist <= minDist)
            {
                minDist = dist;
                nearest = tile;
            }
        }
        return nearest;
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

using System.Collections.Generic;
using UnityEngine;

// 맵 생성 시 돌을 비워 만든 넓은 공간
public class MapRoom
{
    public int id;
    public Vector2Int centerIdx;
    public float radius; // 타일 단위
    public HashSet<Vector2Int> cells = new HashSet<Vector2Int>();

    public Vector2 CenterPosition => MapManager.TileIndexToPosition(centerIdx);

    public MapRoom(int id, Vector2Int centerIdx, float radius)
    {
        this.id = id;
        this.centerIdx = centerIdx;
        this.radius = radius;
    }

    public bool Contains(Vector2 pos)
    {
        return cells.Contains(MapManager.PositionToTileIndex(pos));
    }
}

public class MapRoomCreatedEvent
{
    public MapRoom room;
    public MapRoomCreatedEvent(MapRoom room)
    {
        this.room = room;
    }
}

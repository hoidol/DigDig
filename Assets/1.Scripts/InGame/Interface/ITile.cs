using System.Collections.Generic;
using UnityEngine;
public interface ITile
{
    Vector2Int[,] TileIndexArr
    {
        get;
    }
    void RegisterTile(Vector2Int[,] idxArr);
    void OnDestroy();
    void ReleaseTile();
    Transform Transform
    {
        get;
    }

    public Vector2Int Size
    {
        get;
    }
    [Header("생성 시 타일을 부수면서 등장함")]
    public bool BreakTileWhenSpawn
    {
        get;
    }


}
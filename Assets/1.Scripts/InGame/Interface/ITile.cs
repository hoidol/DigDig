using System.Collections.Generic;
using UnityEngine;
public interface ITile
{
    Vector2Int[,] IndexArr
    {
        get;
    }
    void RegisterTile(Vector2Int[,] idxArr);
    void ReleaseTile();
    Transform Transform
    {
        get;
    }

    public int Size
    {
        get;
    }
    [Header("생성 시 타일을 부수면서 등장함")]
    public bool BreakTileWhenSpawn
    {
        get;
    }


}
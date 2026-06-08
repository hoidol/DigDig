using System.Collections.Generic;
using UnityEngine;
public interface ITile
{
    List<Vector2Int> Indexs
    {
        get;
    }
    void RegisterIndex(Vector2Int index);
    void ReleaseIndex();
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
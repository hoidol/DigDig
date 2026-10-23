using UnityEngine;

// 캐릭터 대각 4칸
public class DiagonalGrantItem : GrantItem
{
    static readonly Vector2Int[] offsets =
    {
        new Vector2Int(-1, 1), new Vector2Int(1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1),
    };
    protected override Vector2Int[] AreaOffsets => offsets;
}

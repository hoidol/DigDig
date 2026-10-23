using UnityEngine;

// 캐릭터 상하좌우 4칸 (십자)
public class AroundFourGrantItem : GrantItem
{
    static readonly Vector2Int[] offsets =
    {
        new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0),
    };
    protected override Vector2Int[] AreaOffsets => offsets;
}

using UnityEngine;

// 캐릭터 주변 8칸 (십자 + 대각)
public class AroundEightGrantItem : GrantItem
{
    static readonly Vector2Int[] offsets =
    {
        new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0),
        new Vector2Int(-1, 1), new Vector2Int(1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1),
    };
    protected override Vector2Int[] AreaOffsets => offsets;
}

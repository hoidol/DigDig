using UnityEngine;

// 개척: 타일에 설치하지 않고, 획득 즉시 막힌 칸 하나를 해제한다
public class PioneerTileItem : TileItem //타일 위에 설치되는 아이템
{
    public override void OnEquip()
    {
        UpdateItem();
        if (!TileManager.Instance.UnblockRandomTile())
            Debug.LogWarning($"PioneerTileItem 해제할 막힌 칸 없음");
    }

    public override void OnUnequip()
    {
    }

    // 막힌 칸이 있을 때만 뽑기 후보
    public override bool CanDraw() => TileManager.Instance.HasBlockedTile;
}

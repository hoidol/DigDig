using UnityEngine;

// 개척: 타일에 설치하지 않고, 획득할 때 고른 막힌 칸(SelectItemCanvas 타일 선택) 하나를 해제한다
// 타일을 지정하지 않고 얻으면(테스트 등) 랜덤 막힌 칸
public class PioneerTileItem : TileItem //타일 위에 설치되는 아이템
{
    public override void OnEquip()
    {
        UpdateItem();
        bool unblocked = tile != null ? TileManager.Instance.UnblockTile(tile) : TileManager.Instance.UnblockRandomTile();
        if (!unblocked)
            Debug.LogWarning($"PioneerTileItem 해제할 막힌 칸 없음");
        tile = null; // 설치하지 않으므로 타일과 연결하지 않음
    }

    public override void OnUnequip()
    {
    }

    // 막힌 칸이 있을 때만 뽑기 후보
    public override bool CanDraw() => TileManager.Instance.HasBlockedTile;

    // 막힌 칸만 고를 수 있음
    public override bool CanPickTile(Tile tile) => tile.isBlocked;
    public override string PickFailMessageKey(Tile tile) => "TileItem_SelectBlockedTile";
}

using UnityEngine;

// 선택된 타일에 설치된 아이템 판매: 제거 후 SELL_TILE_ITEM_PRICE 코인 (TileItemInfoPanel 안에 있음, 아이템 타일 선택 시에만 보임)
public class SellTileItemButton : ButtonUI
{
    Tile tile;

    public void SetTile(Tile tile)
    {
        this.tile = tile;
    }

    // 판매 후 타일 선택 해제 (TilePointer·정보 패널 숨김, 위에 유닛이 있어도 동일)
    public override void OnClickedBtn()
    {
        if (tile == null || tile.tileItem is not TileItem item)
            return;

        KingSlime.Instance.RemoveItem(item);
        KingSlime.Instance.AddCoin(GameSetting.SELL_TILE_ITEM_PRICE);
        SlimeFieldController.Instance.DeselectTile();
    }
}

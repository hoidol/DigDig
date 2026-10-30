using UnityEngine;

// 선택된 타일에 설치된 아이템 판매: 제거 후 SELL_TILE_ITEM_PRICE 코인 (TileItemInfoPanel 안에 있음, 아이템 타일 선택 시에만 보임)
public class SellTileItemButton : ButtonUI
{
    Tile tile;

    public void SetTile(Tile tile)
    {
        this.tile = tile;
    }

    // 제거 후 Tile이 TileItemChangedEvent를 보내 선택 정보가 갱신됨 (아이템 패널 숨김)
    public override void OnClickedBtn()
    {
        if (tile == null || tile.tileItem is not TileItem item)
            return;

        Character.Instance.RemoveItem(item);
        Character.Instance.AddCoin(GameSetting.SELL_TILE_ITEM_PRICE);
        GetComponentInParent<TileItemInfoPanel>().SetTile(null);
    }
}

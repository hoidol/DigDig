using System.Collections.Generic;

// 드워프 · 버프형 · 룬 대장장이: 버프 칸에 새긴 룬의 효과 레벨 +levelBonuses (최대 레벨을 넘김, TileItem.bonusLevel)
// 칸에서 빠지면(킹 이동) 원래 레벨. 룬 강화와 연계
public class DwarfRuneSmithPassive : PassiveKingAbility
{
    public int[] levelBonuses = { 1, 1, 2 };

    readonly HashSet<TileItem> bonused = new HashSet<TileItem>();

    public override object[] GetDescArgs(int level) => new object[] { LV(levelBonuses, level) };

    protected override void Apply()
    {
        Area.TilesRefreshed -= OnTilesRefreshed;
        Area.TilesRefreshed += OnTilesRefreshed;
        GameEventBus.Unsubscribe<TileItemChangedEvent>(OnTileItemChanged);
        GameEventBus.Subscribe<TileItemChangedEvent>(OnTileItemChanged);
        ResetAll();
        Area.Refresh();
    }

    protected override void Clear()
    {
        Area.TilesRefreshed -= OnTilesRefreshed;
        GameEventBus.Unsubscribe<TileItemChangedEvent>(OnTileItemChanged);
        ResetAll();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameEventBus.Unsubscribe<TileItemChangedEvent>(OnTileItemChanged);
    }

    void OnTileItemChanged(TileItemChangedEvent e) => Area.Refresh(); // 룬을 새로 새기거나 지우면 다시 계산

    void ResetAll()
    {
        foreach (TileItem item in bonused)
            if (item != null) item.SetBonusLevel(0);
        bonused.Clear();
    }

    void OnTilesRefreshed(KingTileBuffArea area, List<Tile> tiles)
    {
        var now = new HashSet<TileItem>();
        foreach (Tile tile in tiles)
            if (tile.tileItem is TileItem item && item != null && item.CanUpgrade)
                now.Add(item);
        foreach (TileItem item in bonused)
            if (item != null && !now.Contains(item))
                item.SetBonusLevel(0);
        int bonus = LV(levelBonuses);
        foreach (TileItem item in now)
            item.SetBonusLevel(bonus);
        bonused.Clear();
        bonused.UnionWith(now);
    }
}

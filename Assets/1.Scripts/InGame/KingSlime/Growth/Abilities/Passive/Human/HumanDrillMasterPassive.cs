using System.Collections.Generic;
using UnityEngine;

// 인간 · 버프형 · 훈련 교관: 버프 칸 위 미니언의 데이터 스탯(공격력 등)을 머지 레벨 +bonus 기준으로 계산 (Minion.kingMergeBonus)
// 칸에서 빠지면 원래대로. 최대 머지 레벨을 넘지는 않음 (5성은 효과 없음)
public class HumanDrillMasterPassive : PassiveKingAbility
{
    public int[] mergeBonuses = { 1, 1, 2 };

    readonly HashSet<Minion> bonused = new HashSet<Minion>();

    public override object[] GetDescArgs(int level) => new object[] { LV(mergeBonuses, level) };

    protected override void Apply()
    {
        Area.TilesRefreshed -= OnTilesRefreshed;
        Area.TilesRefreshed += OnTilesRefreshed;
        foreach (Minion m in bonused)
            if (m != null) m.SetKingMergeBonus(0);
        bonused.Clear();
        Area.Refresh();
    }

    protected override void Clear()
    {
        Area.TilesRefreshed -= OnTilesRefreshed;
        foreach (Minion m in bonused)
            if (m != null) m.SetKingMergeBonus(0);
        bonused.Clear();
    }

    void OnTilesRefreshed(KingTileBuffArea area, List<Tile> tiles)
    {
        var now = new HashSet<Minion>(area.GetTargetMinions());
        foreach (Minion m in bonused)
            if (m != null && !now.Contains(m))
                m.SetKingMergeBonus(0);
        int bonus = LV(mergeBonuses);
        foreach (Minion m in now)
            m.SetKingMergeBonus(bonus);
        bonused.Clear();
        bonused.UnionWith(now);
    }
}

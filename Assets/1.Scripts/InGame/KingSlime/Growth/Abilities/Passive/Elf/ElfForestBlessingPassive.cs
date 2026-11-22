// 엘프 · 버프형 · 숲의 축복: 버프 칸 미니언 사거리 +rangeBonuses
public class ElfForestBlessingPassive : PassiveKingAbility
{
    public float[] rangeBonuses = { 1f, 1.5f, 2f };

    public override object[] GetDescArgs(int level) => new object[] { FormatNumber(LV(rangeBonuses, level)) };

    protected override void Apply() =>
        Area.SetBuffs(this, new[] { new Buff(StatType.AttackRange, LV(rangeBonuses), StatOpType.Add) });

    protected override void Clear() => Area.RemoveBuffs(this);
}

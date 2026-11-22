// 엘프 · 공격형 · 정밀 사격: 킹 치명타 확률 +critBonuses%, 킹 치명타에 맞은 적에게 표식 = 모든 아군에게 받는 피해 +markBonuses
// 표식은 낙인(StigmaEffect)과 같은 효과 → 낙인과 겹치면 큰 값만, 적이 죽을 때까지 유지
public class ElfPreciseShotPassive : PassiveKingAbility
{
    public float[] critBonuses = { 15f, 20f, 25f };
    public float[] markBonuses = { 0.15f, 0.22f, 0.3f };

    Buff kingBuff;

    public override object[] GetDescArgs(int level) =>
        new object[] { FormatNumber(LV(critBonuses, level)), Percent(LV(markBonuses, level)) };

    protected override void Apply()
    {
        Clear();
        kingBuff = new Buff(StatType.CritChance, LV(critBonuses), StatOpType.Add);
        king.AddBuff(kingBuff);
    }

    protected override void Clear()
    {
        if (kingBuff != null)
            king.RemoveBuff(kingBuff);
        kingBuff = null;
    }

    public override void OnKingFired(AllyBulletObject bullet)
    {
        float bonus = LV(markBonuses);
        bullet.AddBehavior(new KingHitBehavior((b, enemy) =>
        {
            if (b.DamageData.lastHitCrit && enemy.CurHp > 0)
                enemy.ApplyStatusEffect(new StigmaEffect(bonus));
        }));
    }
}

// 언데드 · 공격형 · 저주: 킹 탄에 맞은 적이 받는 모든 아군 피해 +curseBonuses
// 낙인(StigmaEffect)과 같은 효과 → 낙인·엘프 표식과 겹치면 큰 값만, 적이 죽을 때까지 유지
public class UndeadCursePassive : PassiveKingAbility
{
    public float[] curseBonuses = { 0.1f, 0.175f, 0.25f };

    public override object[] GetDescArgs(int level) => new object[] { Percent(LV(curseBonuses, level)) };

    protected override void Apply() { }

    public override void OnKingFired(AllyBulletObject bullet)
    {
        float bonus = LV(curseBonuses);
        bullet.AddBehavior(new KingHitBehavior((b, enemy) =>
        {
            if (enemy.CurHp > 0)
                enemy.ApplyStatusEffect(new StigmaEffect(bonus));
        }));
    }
}

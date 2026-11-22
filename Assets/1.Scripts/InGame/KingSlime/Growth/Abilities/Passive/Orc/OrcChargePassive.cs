using UnityEngine;

// 오크 · 공격형 · 돌격: 킹 탄이 적을 pierceCounts명까지 관통 + 킹의 중갑 상대 피해 +normalDamageBonuses%
public class OrcChargePassive : PassiveKingAbility
{
    public int[] pierceCounts = { 2, 3, 4 };
    public float[] normalDamageBonuses = { 30f, 45f, 60f };

    Buff kingBuff;

    public override object[] GetDescArgs(int level) =>
        new object[] { LV(pierceCounts, level), FormatNumber(LV(normalDamageBonuses, level)) };

    protected override void Apply()
    {
        Clear();
        kingBuff = new Buff(StatType.DamageToNormal, LV(normalDamageBonuses), StatOpType.Add);
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
        bullet.AddBehavior(new PierceBehavior(LV(pierceCounts), 1f));
    }
}

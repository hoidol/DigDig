using UnityEngine;

// 피뢰침탄: 적중 적에게 표식 부여, 모든 낙뢰가 표식 적을 우선 타격 //낙뢰 + 철
public class LightningRodBullet : Bullet
{
    static readonly float[] MARK_DURATIONS = { 20f, 30f, 40f };
    static readonly float[] THUNDER_RANGES = { 2.0f,3.0f, 4.0f };
    static readonly int[] THUNDER_COUNT = { 3,4, 5 };
    static readonly float[] THUNDER_DAMAGES = { 1.0f, 1.2f, 1.4f };


    public LightningRodBullet() { key = "LightningRod"; }

    public override void OnBulletFired(PlayerBulletObject bullet)
    {
        base.OnBulletFired(bullet);
        bullet.AddBehavior(new LightningRodBehavior(MARK_DURATIONS[GetLevel() - 1]));
        bullet.AddBehavior(new ThunderOnHitBehavior(THUNDER_RANGES[GetLevel() - 1], THUNDER_COUNT[GetLevel() - 1], THUNDER_DAMAGES[GetLevel() - 1]));
    }

    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"적중 적에게 피뢰침 표식 부여 ({MARK_DURATIONS[lv - 1]}초)\n표식된 적은 모든 낙뢰의 우선 타격 대상";
    }
}

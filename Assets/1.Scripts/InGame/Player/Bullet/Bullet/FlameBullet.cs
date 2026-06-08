using UnityEngine;

// [불탄]
// 적중 시 30% 확률로 화상 적용
public class FlameBullet : Bullet
{
    const float BURN_CHANCE = 0.3f;
    const float BURN_DURATION = 3f;
    const float BURN_DPS = 3f;

    public override void OnBulletFired(PlayerBulletObject bullet)
    {

        base.OnBulletFired(bullet);
        bullet.AddBehavior(new ChanceBurnBehavior(BURN_CHANCE, BURN_DURATION, BURN_DPS));

    }

    public override string GetDescription(bool detail = false)
        => $"{BURN_CHANCE * 100:0}% 확률로 화상 ({BURN_DURATION}초 {BURN_DPS} DPS)";

    public override PlayerBulletObject GetBulletObject()
    {
        return null;
    }
}

// FlameBullet 전용 인라인 Behavior (파일 분리 불필요한 단순 래퍼)
public class ChanceBurnBehavior : IBulletBehavior
{
    readonly float chance;
    readonly float duration;
    readonly float dps;

    public ChanceBurnBehavior(float chance, float duration, float dps)
    {
        this.chance = chance;
        this.duration = duration;
        this.dps = dps;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D)
    {
        if (Random.value < chance)
        {
            Debug.Log($"화상 적용! (지속시간: {duration}s, DPS: {dps})");
            hit.ApplyStatusEffect(new BurnEffect(duration, dps));

        }

        return true;
    }

    public void OnMove(BulletObject bullet) { }
    public void Merge(IBulletBehavior other) { }

}

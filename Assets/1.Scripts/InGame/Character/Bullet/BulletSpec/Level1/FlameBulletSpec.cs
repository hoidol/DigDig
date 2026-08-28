using UnityEngine;

// [불탄]
// 적중 시 30% 확률로 화상 적용
public class FlameBulletSpec : BulletSpec
{
    public float burnDuration;
    public float burnDPS;

    public FlameBulletSpec()
    {
        key = "Flame";
    }

    // public override void OnBulletFired(PlayerBulletObject bullet)
    // {

    //     base.OnBulletFired(bullet);
    //     bullet.AddBehavior(new ChanceBurnBehavior(BURN_CHANCE, BURN_DURATIONS[GetLevel()-1], BURN_DPS[GetLevel()-1]));

    // }

    // public override string GetDescription(int lv = 1, bool detail = false)
    //     => $"{BURN_CHANCE * 100:0}% 확률로 화상 ({BURN_DURATIONS[lv-1]}초 {BURN_DPS[lv-1]} DPS)";

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

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (Random.value < chance)
        {
            Debug.Log($"화상 적용! (지속시간: {duration}s, DPS: {dps})");
            hit.ApplyStatusEffect(new FlameEffect(duration, dps));

        }

        return true;
    }

    public void OnMove(BulletObject bullet) { }
    public void Merge(IBulletBehavior other) { }

}

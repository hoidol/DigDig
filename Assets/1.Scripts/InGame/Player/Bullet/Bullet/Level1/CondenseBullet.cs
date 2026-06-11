// [응축탄]
// 튕기는 횟수를 절반으로 줄이고, 줄어든 횟수 × multiplyATK(0.5)만큼 데미지 증가.
// (bounce=10 → 5회, 감소 5 × 0.5 = 2.5× 추가 → 총 3.5×)
public class CondenseBullet : Bullet
{
    public override void OnBulletFired(PlayerBulletObject bullet)
    {
        base.OnBulletFired(bullet); // scale 리셋 포함

        int bounce = Player.Instance.bounce;
        int halfBounce = bounce / 2;
        int reduced = bounce - halfBounce;
        float boost = reduced * bulletData.multiplyATK; // 감소분 × 0.5

        // BounceBehavior를 절반 횟수로 교체
        bullet.ClearBehaviors();
        bullet.AddBehavior(new BounceBehavior(halfBounce));
        bullet.AddBulletForce(new DamageBoostForce(boost));
    }

    public override string GetDescription(bool detail = false)
    {
        int bounce = Player.Instance.bounce;
        int half = bounce / 2;
        float boost = (bounce - half) * 0.5f;
        return $"튕김 {bounce} → {half}회 줄고, 데미지 {1f + boost:0.##}배 상승";
    }

}

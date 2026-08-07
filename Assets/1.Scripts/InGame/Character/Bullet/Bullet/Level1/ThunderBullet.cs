// [낙뢰탄]
// 적중 시 플레이어 주변에서 가장 가까운 적/광석에 낙뢰 (ThunderItem 동일 방식)
public class ThunderBullet : Bullet
{
    const float SEARCH_RADIUS = 3f;
    int[] STRIKE_COUNTS = {2,3,4};
    const float DAMAGE_RATE = 1f; // 공격력의 100%

    public ThunderBullet()
    {
        key = "Thunder";
    }
    // public override void OnBulletFired(PlayerBulletObject bullet)
    // {
    //     base.OnBulletFired(bullet);
    //     bullet.AddBehavior(new ThunderOnHitBehavior(SEARCH_RADIUS, STRIKE_COUNTS[GetLevel()-1], DAMAGE_RATE));
    // }

    // public override string GetDescription(int lv = 1, bool detail = false)
    //     => $"적중 시 낙뢰 (공격력 {DAMAGE_RATE * 100:0}% 데미지, 주변 {SEARCH_RADIUS}m 내 타격)";

}

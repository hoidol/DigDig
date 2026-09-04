// [낙뢰탄]
// 적중 시 플레이어 주변에서 가장 가까운 적/광석에 낙뢰 (ThunderItem 동일 방식)
using UnityEngine;

public class ThunderBulletSpec : BulletSpec
{
    public float searchRadius = 3f;
    public int strikeCount;
    public float damage = 1f; // 공격력의 100%
    public LayerMask hitLayerMask;

    public ThunderBulletSpec()
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

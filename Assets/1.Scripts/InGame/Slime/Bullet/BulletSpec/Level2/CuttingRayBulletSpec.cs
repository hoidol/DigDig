using UnityEngine;

// 레이저탄: 적과 부딪히면 진행 방향으로 길게 레이저 발생
public class CuttingRayBulletSpec : BulletSpec
{
    static readonly float[] LASER_DAMAGE_RATES = { 0.8f, 1.0f, 1.2f };

    public CuttingRayBulletSpec() { key = "CuttingRay"; }

    // public override void OnBulletFired(PlayerBulletObject bullet)
    // {
    //     base.OnBulletFired(bullet);
    //     bullet.AddBehavior(new CuttingRayBehavior(LASER_DAMAGE_RATES[GetLevel() - 1]));
    // }

    // public override string GetDescription(int lv = 1, bool detail = false)
    // {
    //     return $"적 적중 시 진행 방향으로 레이저 발사\n레이저 데미지 공격력의 {LASER_DAMAGE_RATES[lv - 1] * 100:0}%";
    // }
}

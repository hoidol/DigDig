using UnityEngine;

// 강철구: 벽에만 튕김, 튕김 횟수 50% 감소
public class SteelSphereBulletSpec : BulletSpec
{
    public SteelSphereBulletSpec() { key = "SteelSphere"; }

    // public override void OnBulletFired(PlayerBulletObject bullet)
    // {
    //     // 튕김 로직은 SteelSphereBulletObject에서 처리
    // }

    // public override string GetDescription(int lv = 1, bool detail = false)
    // {
    //     return "벽에 닿아야만 튕김 판정\n튕김 횟수 50% 감소, 적 직격 시 즉시 소멸";
    // }
}

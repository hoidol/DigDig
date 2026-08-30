// [회전탄]
// 적중마다 고정 반경으로 1회전 공전. 튕김 횟수만큼 반복 후 소멸.
using UnityEngine;

public class OrbitBulletSpec : AllyBulletSpec
{
    public OrbitBulletSpec()
    {
        key = "Orbit";
    }
    static float[] ORBIT_MULTI_DAMAGES = { 1.2f, 1.3f, 1.5f };

    // public override void OnBulletFired(PlayerBulletObject bullet)
    // {
    //     Orbit orbit = Player.Instance.GetBulletSubTool("Orbit") as Orbit;
    //     orbit.AddOrbitBullet(this);
    //     //bullet.AddBehavior(new OrbitBehavior(Player.Instance.bounce, ORBIT_RADIUS, ANGULAR_SPEED));
    // }
    // public float GetDamage()
    // {
    //     return Player.Instance.statMgr.AttackPower * ORBIT_MULTI_DAMAGES[GetLevel()-1];
    // }
    // public override string GetDescription(int lv = 1, bool detail = false)
    //     => $"적 타격 시 피해 {(ORBIT_MULTI_DAMAGES[lv]-1) *100}% 증가";

}

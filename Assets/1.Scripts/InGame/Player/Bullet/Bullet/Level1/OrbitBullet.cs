// [회전탄]
// 적중마다 고정 반경으로 1회전 공전. 튕김 횟수만큼 반복 후 소멸.
using UnityEngine;

public class OrbitBullet : Bullet
{
    const float ORBIT_RADIUS = 1.5f;
    const float ANGULAR_SPEED = 360f; // 도/초 → 1회전에 1초

    public override void OnBulletFired(PlayerBulletObject bullet)
    {
        Orbit orbit = Player.Instance.GetBulletSubTool("Orbit") as Orbit;
        orbit.AddOrbitBullet(this);
        //bullet.AddBehavior(new OrbitBehavior(Player.Instance.bounce, ORBIT_RADIUS, ANGULAR_SPEED));
    }

    public override string GetDescription(bool detail = false)
        => $"적중마다 반경 {ORBIT_RADIUS} 공전, {Player.Instance.bounce}회 후 소멸";

}

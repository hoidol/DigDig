// 발사 시점에 고정된 비율만큼 데미지를 추가 (ratio=0.5 → 총 1.5×, ratio=-0.5 → 총 0.5×)
using UnityEngine;
public class DamageBoostForce : IBulletForce
{
    readonly float ratio;
    public DamageBoostForce(float ratio) { this.ratio = ratio; }

    public float GetMultiDamage(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        return bullet.damage * ratio;
    }
}

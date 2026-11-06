

// 엘리트/보스에게 추가 피해 (mul 2 → 기본 데미지의 +100%)
using UnityEngine;

public class BigEnemyDamageForce : IBulletForce
{
    public float mul;
    public BigEnemyDamageForce(float mul) => this.mul = mul;

    public float GetMultiDamage(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        Component c = hit as Component;
        if (c != null && (c.GetComponent<EliteEnemy>() != null || c.GetComponent<Boss>() != null))
            return bullet.damage * (mul - 1f);
        return 0;
    }
}
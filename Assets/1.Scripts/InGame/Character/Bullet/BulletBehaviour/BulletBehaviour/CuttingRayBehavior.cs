using UnityEngine;

// 적 적중 시 진행 방향으로 레이저 발사, 선상의 모든 적에게 데미지
public class CuttingRayBehavior : IBulletBehavior
{
    readonly float damageRate;
    const float LASER_RANGE = 100f;

    public CuttingRayBehavior(float damageRate)
    {
        this.damageRate = damageRate;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        float laserDamage = Character.Instance.statMgr.AttackPower * damageRate;
        DamageData dmg = new DamageData { damage = laserDamage };

        RaycastHit2D[] laserHits = Physics2D.RaycastAll(hit2D.point, shootDir, LASER_RANGE, bullet.hitLayerMask);
        foreach (var laserHit in laserHits)
        {
            IHittable target = laserHit.collider.GetComponent<IHittable>();
            if (target == null || target == hit) continue;
            EffectManager.Instance.Play(EffectType.Spark, laserHit.point);
            target.TakeDamage(dmg);
        }

        return true;
    }
}

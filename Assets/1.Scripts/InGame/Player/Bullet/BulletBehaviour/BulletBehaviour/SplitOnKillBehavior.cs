using UnityEngine;

// 처치 시 splitCount 방향으로 분열탄 발사 (튕김 없음, 데미지 절반)
public class SplitOnKillBehavior : IBulletBehavior
{
    readonly int splitCount;
    readonly float damageRatio;

    public SplitOnKillBehavior(int splitCount = 4, float damageRatio = 0.5f)
    {
        this.splitCount = splitCount;
        this.damageRatio = damageRatio;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D)
    {
        if (hit is Enemy enemy && enemy.CurHp <= 0)
        {
            float splitDamage = bullet.damage * damageRatio;
            float angleStep = 360f / splitCount;
            for (int i = 0; i < splitCount; i++)
            {
                float rad = i * angleStep * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                var splitBullet = PlayerBulletObject.Instantiate();
                splitBullet.ClearBehaviors();
                splitBullet.ClearBulletForce();
                splitBullet.transform.position = enemy.transform.position;
                splitBullet.Shoot(dir, splitDamage);
            }
        }
        return true;
    }

    public void OnMove(BulletObject bullet) { }
    public void Merge(IBulletBehavior other) { }
}

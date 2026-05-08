using System.Collections.Generic;
using UnityEngine;

public class AllyBullet : BulletBase
{
    static readonly Queue<AllyBullet> pool = new();
    static AllyBullet prefab;

    public PlayerDamageData damageData = new PlayerDamageData();

    public static AllyBullet Instantiate()
    {
        if (prefab == null)
            prefab = Resources.Load<AllyBullet>("Bullet/AllyBullet");

        if (pool.Count > 0)
        {
            AllyBullet bullet = pool.Dequeue();
            bullet.gameObject.SetActive(true);
            return bullet;
        }

        return Instantiate(prefab);
    }

    public override void Hit(RaycastHit2D hit2D)
    {
        IHittable hit = hit2D.collider.GetComponent<IHittable>();
        if (hit == null) return;
        if (preTarget == hit) return;

        preTarget = hit;
        damageData.Init();
        damageData.cause = transform;
        damageData.damage = damage;
        hit.TakeDamage(damageData);
        Release();
    }

    public override void Release()
    {
        gameObject.SetActive(false);
        pool.Enqueue(this);
    }

}

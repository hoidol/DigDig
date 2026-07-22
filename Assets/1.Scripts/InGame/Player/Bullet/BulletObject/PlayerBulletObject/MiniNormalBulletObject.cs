using System.Collections.Generic;
using UnityEngine;

public class MiniNormalBulletObject : PlayerBulletObject
{
    static readonly Queue<MiniNormalBulletObject> pool = new();
    static MiniNormalBulletObject prefab;

    public override void Shoot(Vector2 dir)
    {
        base.Shoot(dir);
        lifetimeTimer = 2;

        damageData.Init(this);
    }
    public static MiniNormalBulletObject Instantiate()
    {
        if (prefab == null)
            prefab = Resources.Load<MiniNormalBulletObject>("Bullet/MiniNormalBulletObject");

        if (pool.Count > 0)
        {
            MiniNormalBulletObject bullet = pool.Dequeue();
            bullet.gameObject.SetActive(true);
            return bullet;
        }

        return Instantiate(prefab);
    }

    public override IHittable Hit(RaycastHit2D hit2D)
    {
        IHittable hit = hit2D.collider.GetComponent<IHittable>();
        if (hit == null) return null;
        if (preTarget == hit) return null;

        preTarget = hit;
        damageData.Init(this);
        damageData.damage = damage;
        hit.TakeDamage(damageData);
        Release();
        return hit;
    }

    public override void Release()
    {
        gameObject.SetActive(false);
        pool.Enqueue(this);
    }

}
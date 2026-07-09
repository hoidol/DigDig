using System.Collections.Generic;
using UnityEngine;

public class AllyBullet : PlayerBulletObject
{
    static readonly Queue<AllyBullet> pool = new();
    static AllyBullet prefab;


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

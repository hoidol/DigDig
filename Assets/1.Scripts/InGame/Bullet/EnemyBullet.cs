using UnityEngine;
using System.Linq;
using System.Collections.Generic;
public class EnemyBullet : BulletBase
{
    private static Queue<EnemyBullet> pool = new Queue<EnemyBullet>();
    private static EnemyBullet prefab;

    public static EnemyBullet Instantiate()
    {
        if (prefab == null)
            prefab = Resources.Load<EnemyBullet>("Bullet/EnemyBullet");

        if (pool.Count > 0)
        {
            EnemyBullet bullet = pool.Dequeue();
            bullet.gameObject.SetActive(true);
            return bullet;
        }
        else
        {
            return Instantiate(prefab);
        }
    }
    public override void Hit(RaycastHit2D hit2D)
    {

    }

    public override void Release()
    {
        gameObject.SetActive(false);
        pool.Enqueue(this);
    }
    public string[] hitTags;

    public override void CheckHit()
    {
    }
    DamageData damageData = new DamageData();
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hitTags.Contains(other.tag))
        {
            return;
        }
        IHittable hit = other.GetComponent<IHittable>();
        if (hit != null)
        {
            damageData.damage = damage;
            hit.TakeDamage(damageData);
            Release();
        }
    }

}

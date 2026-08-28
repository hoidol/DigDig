using UnityEngine;
using System.Collections.Generic;
public class EnemyBulletObject : BulletObject
{
    private static Queue<EnemyBulletObject> pool = new Queue<EnemyBulletObject>();
    private static EnemyBulletObject prefab;
    public Transform spriteTr;

    public static EnemyBulletObject Instantiate()
    {
        if (prefab == null)
            prefab = Resources.Load<EnemyBulletObject>("Bullet/EnemyBulletObject");

        if (pool.Count > 0)
        {
            EnemyBulletObject bullet = pool.Dequeue();
            bullet.gameObject.SetActive(true);
            return bullet;
        }
        else
        {
            return Instantiate(prefab);
        }
    }
    void Awake()
    {
        damageData = new DamageData();
    }

    public override void Shoot(Vector2 dir, float damage)
    {
        base.Shoot(dir, damage);

        transform.right = dir;
    }
    public override IHittable Hit(RaycastHit2D hit2D)
    {
        return null;
    }

    public override void Release()
    {
        gameObject.SetActive(false);
        pool.Enqueue(this);
    }
    static int playerSideLayer = -1;

    public override void CheckHit() { }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerSideLayer == -1)
            playerSideLayer = LayerMask.NameToLayer("AllyUnit");

        if (other.gameObject.layer != playerSideLayer) return;
        
        if(other.TryGetComponent<IHittable>(out IHittable hit))
        {
            damageData.damage = damage;
            hit.TakeDamage(damageData);
            Release();
        }
    }

}

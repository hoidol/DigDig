using UnityEngine;

using System.Collections.Generic;
public class PlayerBullet : BulletBase
{
    private static Queue<PlayerBullet> pool = new Queue<PlayerBullet>();
    private static PlayerBullet prefab;

    public static PlayerBullet Instantiate()
    {
        if (prefab == null)
            prefab = Resources.Load<PlayerBullet>("Bullet/PlayerBullet");

        if (pool.Count > 0)
        {
            PlayerBullet bullet = pool.Dequeue();
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
        IHittable hit = hit2D.collider.GetComponent<IHittable>();
        if (hit == null)
            return;

        if (preTarget == hit)
            return;

        preTarget = hit;

        float finalDamage = damage;
        var statMgr = Player.Instance.playerStatMgr;
        if (Random.value <= statMgr.CritChance)
            finalDamage *= statMgr.CritPower;
        for (int i = 0; i < forces.Count; i++)
        {
            finalDamage += forces[i].GetMultiDamage(this, hit, hit2D);
        }
        hit.TakeDamage(finalDamage);
        bool shouldRelease = true;
        foreach (var b in behaviors)
        {
            shouldRelease = b.OnHit(this, hit, hit2D); //입사 벡터, 법선 벡터, 전달 필요 
            if (!shouldRelease)
                return;
        }

        if (shouldRelease) Release();
    }
    public override void Release()
    {
        gameObject.SetActive(false);
        pool.Enqueue(this);
    }
    public override void Bounce(RaycastHit2D hit2D)
    {
        direction = Vector2.Reflect(direction, hit2D.normal);
        transform.right = direction;
    }

}


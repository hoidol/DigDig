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
    public override void Hit(IHittable hit)
    {
        base.Hit(hit);
        Effect effect = EffectManager.Instance.Instantiate(EffectType.Hit);
        effect.Play(transform.position, direction);
    }
    public override void Release()
    {
        gameObject.SetActive(false);
        pool.Enqueue(this);
    }

}


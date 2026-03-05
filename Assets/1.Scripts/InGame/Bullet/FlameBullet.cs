using UnityEngine;

using System.Collections.Generic;
public class FlameBullet : BulletBase
{
    private static Queue<FlameBullet> pool = new Queue<FlameBullet>();
    private static FlameBullet prefab;

    public static FlameBullet Instantiate()
    {
        if (prefab == null)
            prefab = Resources.Load<FlameBullet>("Bullet/FlameBullet");

        if (pool.Count > 0)
        {
            FlameBullet bullet = pool.Dequeue();
            bullet.gameObject.SetActive(true);
            return bullet;
        }
        else
        {
            return Instantiate(prefab);
        }
    }

    public override void Release()
    {
        gameObject.SetActive(false);
        pool.Enqueue(this);
    }

}


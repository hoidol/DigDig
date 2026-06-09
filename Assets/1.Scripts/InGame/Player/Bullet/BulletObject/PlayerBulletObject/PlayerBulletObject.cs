using UnityEngine;

using System.Collections.Generic;

public class PlayerBulletObject : BulletObject
{
    private static Queue<PlayerBulletObject> pool = new Queue<PlayerBulletObject>();
    private static PlayerBulletObject prefab;
    public PlayerDamageData damageData = new PlayerDamageData();
    public static PlayerBulletObject Instantiate()
    {
        if (prefab == null)
            prefab = Resources.Load<PlayerBulletObject>("Bullet/PlayerBulletObject");

        if (pool.Count > 0)
        {
            PlayerBulletObject bullet = pool.Dequeue();
            bullet.transform.localScale = Vector3.one;
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
        damageData.Init();
        damageData.cause = transform;

        float finalDamage = damage * damageMultiplier;

        // Debug.Log($"playerBullet Hit 1 finalDamage {finalDamage}");
        for (int i = 0; i < forces.Count; i++)
        {
            // Debug.Log($"playerBullet Hit forces [{i}] {forces[i].GetType().Name} finalDamage {finalDamage}");
            finalDamage += forces[i].GetMultiDamage(this, hit, hit2D);
        }
        if (finalDamage < 1f)
            finalDamage = 1f;

        damageData.Calculate(finalDamage);
        hit.TakeDamage(damageData);
        bool shouldRelease = true;
        foreach (var b in behaviors)
        {
            shouldRelease = b.OnHit(this, hit, hit2D); //입사 벡터, 법선 벡터, 전달 필요 
            if (!shouldRelease)
                return;
        }

        if (shouldRelease) Release();
    }
    public override void Move()
    {
        foreach (var b in behaviors)
            b.OnMove(this);
        base.Move();
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


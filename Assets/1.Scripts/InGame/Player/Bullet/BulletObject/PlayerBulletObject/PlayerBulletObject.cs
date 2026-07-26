using UnityEngine;

using System.Collections.Generic;

public class PlayerBulletObject : BulletObject
{
    // private static Queue<PlayerBulletObject> pool = new Queue<PlayerBulletObject>();
    // private static PlayerBulletObject prefab;
    public string key;
    public PlayerBulletDamageData damageData = new PlayerBulletDamageData();

    protected List<IBulletBehavior> behaviors = new List<IBulletBehavior>();
    protected List<IBulletForce> forces = new List<IBulletForce>();

    public override void Shoot(Vector2 dir)
    {
        base.Shoot(dir);
        damageMultiplier = 1;
        damage = Player.Instance.statMgr.AttackPower;
        lifetimeTimer = Player.Instance.statMgr.AmmoDuration;
        damageData.Init(this);
    }
    public virtual void SetBullet(Bullet bullet)
    {
        damageMultiplier = 1;
        damageData.mustCrit = bullet.mustCrit;
    }

    public override void Update()
    {
        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0)
        {
            Release();
            return;
        }

        Move();
        CheckHit();
    }

    public override IHittable Hit(RaycastHit2D hit2D)
    {
        IHittable hit = hit2D.collider.GetComponent<IHittable>();
        if (hit == null)
            return null;

        if (preTarget == hit)
            return null;

        preTarget = hit;

        float finalDamage = damage * damageMultiplier;

        for (int i = 0; i < forces.Count; i++)
        {
            finalDamage += forces[i].GetMultiDamage(this, hit, hit2D, direction);
        }
        if (finalDamage < 1f)
            finalDamage = 1f;

        damageData.Calculate(finalDamage);
        damageData.hit2D = hit2D;
        hit.TakeDamage(damageData);

        bool shouldRelease = true;
        foreach (var b in behaviors)
        {
            shouldRelease = b.OnHit(this, hit, hit2D, direction); //입사 벡터, 법선 벡터, 전달 필요 
            if (!shouldRelease)
                break;
        }

        if (shouldRelease)
        {
            Debug.Log("PlayerBUlletObject Hit ShouldRelease");
            Release();
        }
        return hit;
    }
    public override void Release()
    {
        gameObject.SetActive(false);
        BulletManager.Instance.ReturnPlayerBulletObject(key, this);
    }


    public void AddBehavior(IBulletBehavior b)
    {
        behaviors.Add(b);
    }
    public void ClearBehaviors() => behaviors.Clear();
    public void AddBulletForce(IBulletForce b)
    {
        forces.Add(b);

    }
    public void RemoveBulletForce(IBulletForce b)
    {
        forces.Remove(b);
    }
    public void ClearBulletForce() => forces.Clear();

    public override void Bounce(RaycastHit2D hit2D)
    {
        Vector2 dir = Vector2.Reflect(direction, hit2D.normal);
        if (dir != Vector2.zero)
            direction = dir;

        transform.right = direction;
    }

}


using UnityEngine;

using System.Collections.Generic;


public class AllyBulletObject : BulletObject
{
    public string key;

    protected List<IBulletBehavior> behaviors = new List<IBulletBehavior>();
    protected List<IBulletForce> forces = new List<IBulletForce>();
    [SerializeField] public AllyUnitDamageData allyUnitDamageData;
    IAllyUnit allyUnit;
    public virtual void SetBullet(BulletSpec bullet, IAllyUnit allyUnit)
    {
        this.allyUnit = allyUnit;
        AllyBulletSpec allyBulletSpec = bullet as AllyBulletSpec;
        damage = allyBulletSpec.damage;
        if (allyUnitDamageData == null)
        {
            allyUnitDamageData = new AllyUnitDamageData();
        }

        // damageData = allyUnitDamageData;
        allyUnitDamageData.damage = damage;

        ClearBehaviors();
        ClearBulletForce();
    }


    public override IHittable Hit(RaycastHit2D hit2D)
    {
        IHittable hit = hit2D.collider.GetComponent<IHittable>();
        if (hit == null)
            return null;

        if (preTarget == hit)
            return null;

        preTarget = hit;

        float finalDamage = damage;

        for (int i = 0; i < forces.Count; i++)
        {
            finalDamage += forces[i].GetMultiDamage(this, hit, hit2D, direction);
        }
        if (finalDamage < 1f)
            finalDamage = 1f;

        allyUnit.AccumulateDamage(finalDamage);
        allyUnitDamageData.damage = finalDamage;
        hit.TakeDamage(allyUnitDamageData);

        bool shouldRelease = true;
        foreach (var b in behaviors)
        {
            shouldRelease = b.OnHit(this, hit, hit2D, direction); //입사 벡터, 법선 벡터, 전달 필요 
            if (!shouldRelease)
                break;
        }

        if (shouldRelease)
        {
            Release();
        }
        return hit;
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
    public override void Release()
    {
        base.Release();
        BulletSpawner.Instance.ReturnPlayerBulletObject(key, this);
    }
}


using UnityEngine;

using System.Collections.Generic;

public class CharacterBulletObject : AllyBulletObject
{
    public CharacterDamageData characterDamageData;

    [field: SerializeField] public float damageMultiplier { get; set; } = 1f;


    public override void Shoot(Vector2 dir,float damage)
    {
        base.Shoot(dir,damage);
        
        damageMultiplier = 1;
        this.damage = damage;
        lifetimeTimer = 20; //Player.Instance.statMgr.AmmoDuration;
        characterDamageData= new CharacterDamageData();
        damageData = characterDamageData;
        characterDamageData.Init(Character.Instance);
    }

    public override void SetBullet(BulletSpec bullet,IAllyUnit allyUnit)
    {
        base.SetBullet(bullet,allyUnit);

        
        damageMultiplier = 1;
        characterDamageData.mustCrit = bullet.mustCrit;
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

        characterDamageData.Calculate(finalDamage);
        characterDamageData.hit2D = hit2D;
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
            Release();
        }
        return hit;
    }

    public override void Release()
    {
        gameObject.SetActive(false);
        BulletSpawner.Instance.ReturnPlayerBulletObject(key, this);
    }

     

}


using UnityEngine;

using System.Collections.Generic;
using Unity.VisualScripting;

public class CharacterBulletObject : AllyBulletObject
{
    bool mustCrit;

    // [field: SerializeField] public float damageMultiplier { get; set; } = 1f;


    public override void Shoot(Vector2 dir, float damage)
    {
        base.Shoot(dir, damage);

        this.damage = damage;
        lifetimeTimer = 5; //Player.Instance.statMgr.AmmoDuration;

        // 캐릭터 치명타 스탯 (타일 치명타는 이후 OnFired에서 확률이 더해짐)
        allyUnitDamageData.critChance = mustCrit ? 100f : Character.Instance.statMgr.CritChance;
        allyUnitDamageData.critPower = Character.Instance.statMgr.CritPower;
    }

    public override void SetBullet(BulletSpec bullet, IAllyUnit allyUnit)
    {
        base.SetBullet(bullet, allyUnit);
        allyUnitDamageData.damage = (bullet as CharacterBulletSpec).damage;
        allyUnitDamageData.attackType = AttackType.Physic; // 캐릭터 총알은 물리

        damageMultiplier = 1;
        mustCrit = bullet.mustCrit;
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
}

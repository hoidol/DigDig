using UnityEngine;

using System.Collections.Generic;
using Unity.VisualScripting;

public class KingSlimeBulletObject : AllyBulletObject
{
    bool mustCrit;

    // [field: SerializeField] public float damageMultiplier { get; set; } = 1f;


    public override void Shoot(Vector2 dir, float damage)
    {
        base.Shoot(dir, damage);

        this.damage = damage;
        lifetimeTimer = 5; //Player.Instance.statMgr.AmmoDuration;

        // 캐릭터 치명타 스탯 (타일 치명타는 이후 OnFired에서 확률이 더해짐)
        allyUnitDamageData.critChance = mustCrit ? 100f : KingSlime.Instance.statMgr.CritChance;
        allyUnitDamageData.critPower = KingSlime.Instance.statMgr.CritPower;
    }

    public override void SetBullet(BulletSpec bullet, IAllyUnit allyUnit)
    {
        base.SetBullet(bullet, allyUnit);
        allyUnitDamageData.damage = (bullet as KingSlimeBulletSpec).damage;
        allyUnitDamageData.attackType = allyUnit.AttackType; // (2026-11-21) 킹 공격 타입 = 무기 설정(Weapon.attackType, 지금 카오스). 이전: 물리 고정

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

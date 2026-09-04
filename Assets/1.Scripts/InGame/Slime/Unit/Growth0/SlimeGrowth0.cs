using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public abstract class SlimeGrowth0 : Slime
{

    public float attackPower;
    public float attackSpeed;
    AllyBulletSpec allyBulletSpec;

    public override void Awake()
    {
        base.Awake();
        attackPower = 2;
        attackSpeed = 1;
        allyBulletSpec = new AllyBulletSpec();
        allyBulletSpec.damage = attackPower;
    }

    public override float AttackPower()
    {
        return attackPower;
    }

    public override float AttackSpeed()
    {
        return attackSpeed;
    }

    public override AllyBulletObject GetBullet()
    {
        return allyBulletSpec.Instantiate(this);
    }

}

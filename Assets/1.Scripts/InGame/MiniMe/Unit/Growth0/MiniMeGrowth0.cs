using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public abstract class MiniMeGrowth0 : MiniMe
{
    
    public float attackPower;
    public float attackSpeed;
    NormalBulletSpec normalBulletSpec;

     public override void Awake()
    {
        base.Awake();
        attackPower = 3;
        attackSpeed = 1;
        normalBulletSpec= new NormalBulletSpec();
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
        return normalBulletSpec.Instantiate(this);
    }

}

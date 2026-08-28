using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public class MiniMeGrowth2 : MiniMe
{
    public float attackPower;
    public float attackSpeed;

    public override float AttackPower()
    {
        return attackPower;
    }

    public override float AttackSpeed()
    {
        return attackSpeed;
    }

}

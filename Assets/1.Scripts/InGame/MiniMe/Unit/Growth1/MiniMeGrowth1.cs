using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public abstract class MiniMeGrowth1 : MiniMe
{
    public float[] attackPowers;
    public float[] attackSpeeds;

    public override float AttackPower()
    {
        return attackPowers[level];
    }

    public override float AttackSpeed()
    {
        return attackSpeeds[level];;
    }

}

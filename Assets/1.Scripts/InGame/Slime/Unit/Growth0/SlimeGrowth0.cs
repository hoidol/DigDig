using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public abstract class SlimeGrowth0 : Slime
{
    AllyBulletSpec allyBulletSpec;

    public override void Spawn(Vector2 pos, int mLv)
    {
        base.Spawn(pos, mLv);
        allyBulletSpec = new AllyBulletSpec();

    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }

}

using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public class HoverSlime : SlimeGrowth1
{
    public OrbitMachine orbitMachine;
    public float orbitDamage;

    public override AllyBulletObject GetBullet()
    {
        throw new System.NotImplementedException();
    }

    public override string GetDescription(int level =0)
    {
        throw new System.NotImplementedException();
    }

    // public override void Spawn(Vector2 pos)
    // {
    //     base.Spawn(pos);
    //     orbitMachine.damage = orbitDamage;
    //     orbitMachine.AddOrbit();
    //     orbitMachine.AddOrbit();
    // }

    public void UpdateSlime()
    {
        for (int i = 0; i < orbitMachine.orbitOrbs.Count; i++)
        {
            orbitMachine.orbitOrbs[i].damage = orbitDamage;
        }
    }
}
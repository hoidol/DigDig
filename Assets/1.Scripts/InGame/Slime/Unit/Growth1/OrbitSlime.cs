using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public class OrbitSlime : SlimeGrowth1
{
    public OrbitMachine orbitMachine;
    // public float[] orbitDamages = {3,3,3};
    public float[] orbitRotationSpeeds = {120,150,180};


    public override void Awake()
    {
        base.Awake();
        

    }
    
    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);

        orbitMachine.damage = AttackPower;
        orbitMachine.radius = 2;
        orbitMachine.rotationSpeed = orbitRotationSpeeds[level];
        for(int i = 0; i < level + 1; i++)
        {
            orbitMachine.AddOrbit();    
        }
    }


    public override AllyBulletObject GetBullet()
    {
        throw new System.NotImplementedException();
    }

    public override string GetDescription(int level =0)
    {
        throw new System.NotImplementedException();
    }

}
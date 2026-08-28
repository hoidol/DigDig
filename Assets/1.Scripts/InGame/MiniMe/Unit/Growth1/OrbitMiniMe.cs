using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public class OrbitMiniMe : MiniMeGrowth1
{
    public OrbitMachine orbitMachine;
    // public float[] orbitDamages = {3,3,3};
    public float[] orbitRotationSpeeds = {120,150,180};

    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);
    }


    public override void Awake()
    {
        base.Awake();
        
        attackPowers = new float[] {3,4,5};
        attackSpeeds = new float[] {30,30,30};

        orbitMachine.damage = attackPowers[level];
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

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

}
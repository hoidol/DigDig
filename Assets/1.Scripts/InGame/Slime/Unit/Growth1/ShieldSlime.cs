using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public class ShieldSlime : SlimeGrowth1
{
    public OrbitMachine orbitMachine;
    public float orbitRotationSpeed = 120;

    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);
    }


    public override void Awake()
    {
        base.Awake();
        
        attackPowers = new float[] {3,4,5};
        attackSpeeds = new float[] {10,10,10};

        orbitMachine.damage = attackPowers[level];
        orbitMachine.radius = 2.3f;
        orbitMachine.rotationSpeed = orbitRotationSpeed;
        for(int i = 0; i < level + 1; i++)
        {
            orbitMachine.AddOrbit();    
        }
    }
    

    public override AllyBulletObject GetBullet()
    {
        return null;
    }

    public override string GetDescription(int level =0)
    {
        return "쉴드로 적의 공격을 막습니다.";
    }

}
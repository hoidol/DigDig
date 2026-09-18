using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public class OrbitSlime : SlimeGrowth1
{
    public OrbitMachine orbitMachine;
    public OrbitSlimeData  orbitSlimeData;
    public int orbitCount ;
    public float rotationSpeed ;
    public  float radius;

    public OrbitEnhance8Ability pushAbility;
    public override void Spawn(Vector2 pos, int lv)
    {
        orbitSlimeData = SlimeManager.Instance.GetSlimeData(key) as OrbitSlimeData;
        base.Spawn(pos, lv);        
    }

    public override void InitSlime()
    {        
        orbitCount = orbitSlimeData.orbitCount[mergeLevel];
        rotationSpeed = orbitSlimeData.orbitRotationSpeed;
        radius = orbitSlimeData.radius;

        base.InitSlime();

        //회전 머신 설정
        orbitMachine.damage = AttackPower;
        orbitMachine.radius = radius;
        orbitMachine.rotationSpeed = rotationSpeed;
        for(int i = 0; i < orbitCount; i++)
        {
            OrbitOrb orbitOrb = orbitMachine.AddOrbit();
            if (pushAbility.isActivate)
            {
                orbitOrb.AddBulletBehaviour(new PushBehavior(pushAbility.power));
            }
        }
    }

    public override void UpdateSlime()
    {
        base.UpdateSlime();
        orbitMachine.damage = AttackPower;
        orbitMachine.UpdateOrbitMachine();
    }
    public override void Fire(Vector2 dir)
    {
        
    }

    public override AllyBulletObject GetBullet()
    {
        return null;
    }
}
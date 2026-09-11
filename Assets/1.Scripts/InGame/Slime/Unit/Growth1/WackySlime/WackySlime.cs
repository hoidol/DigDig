using UnityEngine;

public class WackySlime : SlimeGrowth1
{
    float[] fireTimes = { 2f, 1.5f, 1f };
    float fireTimer;
    AllyBulletSpec allyBulletSpec;
    public float wackyFireSpeed;

    public override void Awake()
    {
        base.Awake();
        allyBulletSpec = new AllyBulletSpec();
    }

    public override void Spawn(Vector2 pos, int lv)
    {
        allyBulletSpec = new AllyBulletSpec();
        allyBulletSpec.damage = AttackPower;
    
        fireTimer = 0;
        wackyFireSpeed = 1;
        base.Spawn(pos, lv);

    }


    public override void Update()
    {
        base.Update();
        if (fireTimer >= fireTimes[level])
        {
            Fire();
            fireTimer = 0;
        }
        fireTimer += Time.deltaTime * wackyFireSpeed;
    }

    void Fire()
    {
        for (int i = 0; i < level + 1; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Fire(randomDir);
        }
    }

    public override AllyBulletObject GetBullet()
    {
        return allyBulletSpec.Instantiate(this);
    }

    public override string GetDescription(int level =0)
    {
        return "랜덤한 방향으로 총알을 발사합니다.";
    }
}
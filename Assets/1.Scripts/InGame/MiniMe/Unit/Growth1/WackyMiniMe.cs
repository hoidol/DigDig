using UnityEngine;

public class WackyMiniMe : MiniMeGrowth1
{
    float[] fireTimes = { 1, 1f, 1f };
    float fireTimer;
    AllyBulletSpec allyBulletSpec;

    public override void Awake()
    {
        base.Awake();
        attackPowers = new float[] { 6, 20, 30 };
        attackSpeeds = new float[] { 10, 20, 30 };
        allyBulletSpec = new AllyBulletSpec();
    }

    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);
        fireTimer = 0;
    }

    public override void Update()
    {
        base.Update();
        if (fireTimer >= fireTimes[level])
        {
            Fire();
            fireTimer = 0;
        }
        fireTimer += Time.deltaTime;
    }

    void Fire()
    {
        for (int i = 0; i < level + 1; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            attackBehaviour.Fire(randomDir);
        }
    }

    public override AllyBulletObject GetBullet()
    {
        return allyBulletSpec.Instantiate(this);
    }

    public override string GetDescription()
    {
        return "랜덤한 방향으로 총알을 발사합니다.";
    }
}
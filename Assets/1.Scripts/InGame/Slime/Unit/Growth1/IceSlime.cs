using UnityEngine;

public class IceSlime : SlimeGrowth1
{
    IceBulletSpec iceBulletSpec;
    float[] durations = {3f,4.5f,6f};
    public override void Awake()
    {
        base.Awake();
        UserSlime userSlime = UserManager.Instance.userSlimeManager.GetUserSlime(key);
        
        userSlime.EnhanceLevel();

        attackPowers = new float[] {4,6,8};
        attackSpeeds = new float[] {2,2,2};


        iceBulletSpec = new IceBulletSpec();
        iceBulletSpec.duration = durations[level];
    }

    public override AllyBulletObject GetBullet()
    {
        return iceBulletSpec.Instantiate(this);
    }

    public override string GetDescription(int level =0)
    {
        return "얼음을 쏩니다";
    }
}
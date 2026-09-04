using UnityEngine;

public class BoomSlime : SlimeGrowth1
{
    BoomBulletSpec boomBulletSpec;
    float[] boomRanges = {2f,2.5f,3f};
    public override void Awake()
    {
        base.Awake();
        UserSlime userSlime = UserManager.Instance.userSlimeManager.GetUserSlime(key);
        
        userSlime.EnhanceLevel();

        attackPowers = new float[] {4,6,8};
        attackSpeeds = new float[] {5,4.7f,4.2f};

        boomBulletSpec = new BoomBulletSpec();
        boomBulletSpec.boomRange = boomRanges[level];
        boomBulletSpec.damage = attackPowers[level];
    }

    public override AllyBulletObject GetBullet()
    {
        return boomBulletSpec.Instantiate(this);
    }

    public override string GetDescription(int level =0)
    {
        return "폭탄을 발사합니다.";
    }
}
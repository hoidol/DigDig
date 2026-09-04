using UnityEngine;

public class LightningSlime : SlimeGrowth1
{
    LightningBulletSpec lightningBulletSpec;
    int[] lightningCounts = {3,5,7};
    public LayerMask hitLayerMask;
    float searchRadius = 2f;
    float initSearchRadius = 6f;
    public override void Awake()
    {
        base.Awake();
        UserSlime userSlime = UserManager.Instance.userSlimeManager.GetUserSlime(key);
        
        userSlime.EnhanceLevel();

        attackPowers = new float[] {4,6,8};
        attackSpeeds = new float[] {10,20,30};


        lightningBulletSpec = new LightningBulletSpec();
        lightningBulletSpec.initSearchRadius = initSearchRadius;
        lightningBulletSpec.searchRadius = searchRadius;
        lightningBulletSpec.lightningCount = lightningCounts[level];
        lightningBulletSpec.damage = attackPowers[level];
        lightningBulletSpec.hitLayerMask = hitLayerMask;
    }

    public override AllyBulletObject GetBullet()
    {
        return lightningBulletSpec.Instantiate(this);
    }


    public override string GetDescription(int level =0)
    {
        return "번개를 쏩니다";
    }
}
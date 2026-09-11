using UnityEngine;

public class LightningSlime : SlimeGrowth1
{
    LightningBulletSpec lightningBulletSpec;
    int[] lightningCounts = {3,5,7};
    public LayerMask hitLayerMask;
    float searchRadius = 2f;
    float initSearchRadius = 6f;

    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);

        
        lightningBulletSpec = new LightningBulletSpec();
        lightningBulletSpec.initSearchRadius = initSearchRadius;
        lightningBulletSpec.searchRadius = searchRadius;
        lightningBulletSpec.lightningCount = lightningCounts[level];
        lightningBulletSpec.damage = AttackPower;
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
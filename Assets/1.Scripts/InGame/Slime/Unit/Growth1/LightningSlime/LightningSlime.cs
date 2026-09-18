using UnityEngine;

public class LightningSlime : SlimeGrowth1
{
    public LightningSlimeData lightningSlimeData;
    LightningBulletSpec lightningBulletSpec;
    public float searchRadius = 2f;
    public float initSearchRadius = 6f;
    public int lightningCount;
    public LightningEnhance8Ability slowEffect;

    public override void Spawn(Vector2 pos, int lv)
    {
        lightningSlimeData = SlimeManager.Instance.GetSlimeData(key) as LightningSlimeData;
        base.Spawn(pos, lv);        
    }

    public override void InitSlime()
    {
        lightningBulletSpec = new LightningBulletSpec();

        initSearchRadius = lightningSlimeData.initSearchRadius;
        searchRadius = lightningSlimeData.searchRadius;
        lightningCount = lightningSlimeData.lightningCounts[mergeLevel];
        lightningBulletSpec.hitLayerMask = targetLayerMask;

        base.InitSlime();
    }

    public override AllyBulletObject GetBullet()
    {
        lightningBulletSpec.initSearchRadius = initSearchRadius;
        lightningBulletSpec.searchRadius = searchRadius;
        lightningBulletSpec.lightningCount = lightningCount;
        lightningBulletSpec.damage = AttackPower;
        lightningBulletSpec.StartBulletSpec();
        if (slowEffect.isActivate)
        {
            lightningBulletSpec.AddBulletBehaviour(new SlowOnHitBehavior(1,slowEffect.slowDuration));
        }        

        return lightningBulletSpec.Instantiate(this);
    }
}
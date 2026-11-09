using UnityEngine;

// [각성] (진화) 감전: 맞은 적 EVOLVE_SLOW_DURATION초 감속
public class LightningSlime : SlimeGrowth1
{
    public const float EVOLVE_SLOW_DURATION = 1f;
    public LightningSlimeData lightningSlimeData;
    [SerializeField] LightningBulletSpec lightningBulletSpec;
    public float searchRadius = 2f;
    public float initSearchRadius = 6f;
    public int lightningCount;

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
        lightningCount = lightningSlimeData.lightningCounts.GetByMergeLevel(mergeLevel);
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
        if (isEvolved)
            lightningBulletSpec.AddBulletBehaviour(new SlowOnHitBehavior(1, EVOLVE_SLOW_DURATION));

        return lightningBulletSpec.Instantiate(this);
    }
}
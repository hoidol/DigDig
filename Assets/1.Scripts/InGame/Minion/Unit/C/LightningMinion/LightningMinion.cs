using UnityEngine;

// [각성] (진화) 감전: 맞은 적 EVOLVE_SLOW_DURATION초 감속
public class LightningMinion : MinionGrowth1
{
    public const float EVOLVE_SLOW_DURATION = 1f;
    public LightningMinionData lightningMinionData;
    [SerializeField] LightningBulletSpec lightningBulletSpec;
    public float searchRadius = 2f;
    public float initSearchRadius = 6f;
    public int lightningCount;

    public override void Spawn(Vector2 pos, int lv)
    {
        lightningMinionData = MinionManager.Instance.GetMinionData(key) as LightningMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        lightningBulletSpec = new LightningBulletSpec();

        initSearchRadius = lightningMinionData.initSearchRadius;
        searchRadius = lightningMinionData.searchRadius;
        lightningCount = lightningMinionData.lightningCounts.GetByMergeLevel(mergeLevel);
        lightningBulletSpec.hitLayerMask = targetLayerMask;

        base.InitMinion();
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
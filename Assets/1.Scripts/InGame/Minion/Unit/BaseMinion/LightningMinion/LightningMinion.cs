using UnityEngine;

// (2026-11-26) 진화 = MinionEvolutionSetting + EvolutionMinion/Behaviours (예전 [각성] 효과 삭제)
public class LightningMinion : Minion
{
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

        return lightningBulletSpec.Instantiate(this);
    }
}
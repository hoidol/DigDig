using UnityEngine;

// [각성] (진화) 얼음 탄이 EVOLVE_BOUNCE_COUNT회 튕김
public class IceMinion : Minion
{
    public const int EVOLVE_BOUNCE_COUNT = 1;
    IceBulletSpec iceBulletSpec;
    IceMinionData iceMinionData;
    public float iceDuration;
    public float iceSlowRate;

    public override void Spawn(Vector2 pos, int lv)
    {
        iceMinionData = MinionManager.Instance.GetMinionData(key) as IceMinionData;
        base.Spawn(pos, lv);
    }

    // (2026-11-24) 얼음(눈송이)은 가까운 적이 기본 (빙결·감속을 가까이 온 적에게)
    public override FindTargetType DefaultFindTargetType => FindTargetType.Closest;

    public override void InitMinion()
    {
        iceBulletSpec = new IceBulletSpec();

        iceDuration = iceMinionData.durations.GetByMergeLevel(mergeLevel);
        iceSlowRate = iceMinionData.slowRate.GetByMergeLevel(mergeLevel);
        base.InitMinion();
    }

    public override AllyBulletObject GetBullet()
    {
        iceBulletSpec.damage = AttackPower;
        iceBulletSpec.duration = iceDuration;
        iceBulletSpec.slowRate = iceSlowRate;

        iceBulletSpec.StartBulletSpec();
        if (isEvolved)
            iceBulletSpec.AddBulletBehaviour(new BounceBehavior(EVOLVE_BOUNCE_COUNT, 1));
        return iceBulletSpec.Instantiate(this);
    }
}

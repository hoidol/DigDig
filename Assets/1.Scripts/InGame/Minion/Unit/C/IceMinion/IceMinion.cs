using UnityEngine;

// [각성] (진화) 얼음 탄이 EVOLVE_BOUNCE_COUNT회 튕김
public class IceMinion : MinionGrowth1
{
    public const int EVOLVE_BOUNCE_COUNT = 1;
    IceBulletSpec iceBulletSpec;
    IceMinionData iceMinionData;
    public float iceDuration;

    public override void Spawn(Vector2 pos, int lv)
    {
        iceMinionData = MinionManager.Instance.GetMinionData(key) as IceMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        iceBulletSpec = new IceBulletSpec();

        iceDuration = iceMinionData.durations.GetByMergeLevel(mergeLevel);
        base.InitMinion();
    }

    public override AllyBulletObject GetBullet()
    {
        iceBulletSpec.damage = AttackPower;
        iceBulletSpec.duration = iceDuration;

        iceBulletSpec.StartBulletSpec();
        if (isEvolved)
            iceBulletSpec.AddBulletBehaviour(new BounceBehavior(EVOLVE_BOUNCE_COUNT, 1));
        return iceBulletSpec.Instantiate(this);
    }
}

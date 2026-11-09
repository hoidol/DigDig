using UnityEngine;

// [각성] (진화) 얼음 탄이 EVOLVE_BOUNCE_COUNT회 튕김
public class IceSlime : SlimeGrowth1
{
    public const int EVOLVE_BOUNCE_COUNT = 1;
    IceBulletSpec iceBulletSpec;
    IceSlimeData iceSlimeData;
    public float iceDuration;

    public override void Spawn(Vector2 pos, int lv)
    {
        iceSlimeData = SlimeManager.Instance.GetSlimeData(key) as IceSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        iceBulletSpec = new IceBulletSpec();

        iceDuration = iceSlimeData.durations.GetByMergeLevel(mergeLevel);
        base.InitSlime();
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

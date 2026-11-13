using UnityEngine;

// [각성] (진화) 탄 피해 +EVOLVE_DAMAGE_BONUS
public class BounceMinion : MinionGrowth1
{
    public const float EVOLVE_DAMAGE_BONUS = 0.2f;
    BounceBulletSpec bounceBulletSpec;
    public int bounceCount;
    public BounceMinionData bounceMinionData;
    public override void Spawn(Vector2 pos, int lv)
    {
        bounceMinionData = MinionManager.Instance.GetMinionData(key) as BounceMinionData;
        base.Spawn(pos, lv);
    }

    // public override int ShootCount => bounceMinionData.shootCounts.GetByMergeLevel(mergeLevel);

    public override void InitMinion()
    {
        bounceCount = bounceMinionData.bounces.GetByMergeLevel(mergeLevel);
        bounceBulletSpec = new BounceBulletSpec();

        base.InitMinion();
    }

    public override AllyBulletObject GetBullet()
    {
        bounceBulletSpec.bounce = bounceCount;
        bounceBulletSpec.damage = AttackPower;
        bounceBulletSpec.multi = 0.8f;
        bounceBulletSpec.StartBulletSpec();
        if (isEvolved)
            bounceBulletSpec.AddBulletForce(new DamageBoostForce(EVOLVE_DAMAGE_BONUS));

        return bounceBulletSpec.Instantiate(this);
    }


}
using UnityEngine;

// 천둥 슬라임: 타겟 위치에 낙뢰를 떨어뜨려 주변 범위 피해 (탄이 날아가지 않음)
// 낙뢰 범위는 머지 레벨로 커짐 (ThunderMinionData.strikeRadiuses)
// [각성] (진화) 낙뢰에 맞은 적 EVOLVE_SLOW_CHANCE 확률로 EVOLVE_SLOW_DURATION초 감속
public class ThunderMinion : MinionGrowth1
{
    public const float EVOLVE_SLOW_CHANCE = 0.1f; // 0~1
    public const float EVOLVE_SLOW_DURATION = 1f;
    ThunderMinionData thunderMinionData;
    ThunderBulletSpec thunderBulletSpec;
    public float strikeRadius;
    public int maxHitCount;

    public override void Spawn(Vector2 pos, int lv)
    {
        thunderMinionData = MinionManager.Instance.GetMinionData(key) as ThunderMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        thunderBulletSpec = new ThunderBulletSpec();
        strikeRadius = thunderMinionData.strikeRadiuses.GetByMergeLevel(mergeLevel);
        maxHitCount = thunderMinionData.maxHitCounts[mergeLevel];
        base.InitMinion();
    }

    public override AllyBulletObject GetBullet()
    {
        thunderBulletSpec.damage = AttackPower;
        thunderBulletSpec.strikeRadius = strikeRadius;
        thunderBulletSpec.maxHitCount = maxHitCount;
        thunderBulletSpec.maxDistance = AttackRange;
        thunderBulletSpec.hitLayerMask = targetLayerMask;
        thunderBulletSpec.StartBulletSpec();
        if (isEvolved)
            thunderBulletSpec.AddBulletBehaviour(new SlowOnHitBehavior(EVOLVE_SLOW_CHANCE, EVOLVE_SLOW_DURATION));

        return thunderBulletSpec.Instantiate(this);
    }
}

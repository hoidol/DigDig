using UnityEngine;

// 천둥 슬라임: 타겟 위치에 낙뢰를 떨어뜨려 주변 범위 피해 (탄이 날아가지 않음)
// 낙뢰 범위는 머지 레벨로 커짐 (ThunderSlimeData.strikeRadiuses)
public class ThunderSlime : SlimeGrowth1
{
    ThunderSlimeData thunderSlimeData;
    ThunderBulletSpec thunderBulletSpec;
    public float strikeRadius;
    public int maxHitCount;
    public ThunderEnhance8Ability slowAbility; // [각성] 낙뢰 맞은 적 감속

    public override void Spawn(Vector2 pos, int lv)
    {
        thunderSlimeData = SlimeManager.Instance.GetSlimeData(key) as ThunderSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        thunderBulletSpec = new ThunderBulletSpec();
        strikeRadius = thunderSlimeData.strikeRadiuses.GetByMergeLevel(mergeLevel);
        maxHitCount = thunderSlimeData.maxHitCounts[mergeLevel];
        base.InitSlime(); // 강화 능력이 범위/타격 수를 바꿀 수 있으므로 기본값 설정 후 호출
    }

    public override AllyBulletObject GetBullet()
    {
        thunderBulletSpec.damage = AttackPower;
        thunderBulletSpec.strikeRadius = strikeRadius;
        thunderBulletSpec.maxHitCount = maxHitCount;
        thunderBulletSpec.maxDistance = AttackRange;
        thunderBulletSpec.hitLayerMask = targetLayerMask;
        thunderBulletSpec.StartBulletSpec();
        if (slowAbility != null && slowAbility.isActivate)
            thunderBulletSpec.AddBulletBehaviour(new SlowOnHitBehavior(slowAbility.slowChance, slowAbility.slowDuration));

        return thunderBulletSpec.Instantiate(this);
    }
}

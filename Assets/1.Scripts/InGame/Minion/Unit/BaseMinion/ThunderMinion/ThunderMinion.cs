using UnityEngine;

// 천둥 슬라임: 타겟 위치에 낙뢰를 떨어뜨려 주변 범위 피해 (탄이 날아가지 않음)
// 낙뢰 범위는 머지 레벨로 커짐 (ThunderMinionData.strikeRadiuses)
// (2026-11-26) 진화 = ThunderEvolution (구름 3개 번개, MinionEvolutionSetting.thunder). 예전 [각성] 감속 삭제
public class ThunderMinion : Minion
{
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

        return thunderBulletSpec.Instantiate(this);
    }
}

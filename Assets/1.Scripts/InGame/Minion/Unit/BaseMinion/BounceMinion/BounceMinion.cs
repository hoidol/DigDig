using UnityEngine;

// 스킬(2026-11-15): SKILL_COOLTIME초마다 SKILL_BULLET_COUNT방향으로 도탄 원형 발사 (첫 방향 = 사거리 안 가장 가까운 적, 적이 없으면 찬 채로 대기)
// 기본 탄 = 연쇄 바운드 (BounceBehavior: 맞으면 주변 가장 가까운 다른 적을 쫓아가 타격)
// [각성] (진화) 탄 피해 +EVOLVE_DAMAGE_BONUS
public class BounceMinion : Minion
{
    public const float EVOLVE_DAMAGE_BONUS = 0.2f;
    public const float SKILL_COOLTIME = 8f;
    public const int SKILL_BULLET_COUNT = 3;
    float skillTimer;

    public override float SkillProgress => Mathf.Clamp01(skillTimer / SKILL_COOLTIME);
    public override void FillSkillForTest() { skillTimer = SKILL_COOLTIME; }
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
        skillTimer = 0;

        base.InitMinion();
    }

    public override void Update()
    {
        base.Update();
        if (IsBeingDragged) // 드래그 중엔 스킬 발동 안 함
            return;
        if (skillTimer < SKILL_COOLTIME)
        {
            skillTimer += SkillDeltaTime;
            return;
        }
        Transform target = FindClosestTargetInRange();
        if (target == null)
            return;
        skillTimer = 0;
        FireCircleAt(target, SKILL_BULLET_COUNT);
        PlaySkillAnim();
    }

    public override AllyBulletObject GetBullet()
    {
        bounceBulletSpec.bounce = bounceCount;
        bounceBulletSpec.damage = AttackPower;
        bounceBulletSpec.multi = 0.7f;
        bounceBulletSpec.StartBulletSpec();
        if (isEvolved)
            bounceBulletSpec.AddBulletForce(new DamageBoostForce(EVOLVE_DAMAGE_BONUS));

        return bounceBulletSpec.Instantiate(this);
    }


}
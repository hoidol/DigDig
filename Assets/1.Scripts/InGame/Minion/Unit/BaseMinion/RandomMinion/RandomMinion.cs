using UnityEngine;

// 공격력 랜덤 폭: 데이터에는 중간값만 (MinionData.attackPowerRandomRatio)
// 스킬(쿨타임, MinionData.hasSkill, 2026-11-15): SKILL_COOLTIME초마다 SKILL_DURATION초 동안 공속 × SKILL_ATTACK_SPEED_MUL
//   쿨타임이 차면 적이 없어도 바로 발동. 버프가 끝나면 쿨타임 다시 참 (이전: 10초마다 5방향 원형 발사)
// [각성] (진화) EVOLVE_SKILL_INTERVAL번째 공격마다 EVOLVE_SKILL_BULLET_COUNT방향으로 원형 발사
public class RandomMinion : Minion
{
    public const float SKILL_COOLTIME = 9f;
    public const float SKILL_DURATION = 3f;
    public const float SKILL_ATTACK_SPEED_MUL = 3f;
    public const int EVOLVE_SKILL_INTERVAL = 6;
    public const int EVOLVE_SKILL_BULLET_COUNT = 4;

    public RandomMinionData randomMinionData;
    AllyBulletSpec allyBulletSpec;
    int attackCount;
    float skillTimer;
    // 스킬 공속 버프 (AddBuff로 걸고 skillBuffTimer가 끝나면 RemoveBuff)
    Buff skillBuff;
    float skillBuffTimer;
    bool IsSkillBuffActive => skillBuffTimer > 0f;

    public override float SkillProgress => IsSkillBuffActive ? 0f : Mathf.Clamp01(skillTimer / SKILL_COOLTIME);
    public override void FillSkillForTest() { skillTimer = SKILL_COOLTIME; }

    public override void Spawn(Vector2 pos, int lv)
    {
        randomMinionData = MinionManager.Instance.GetMinionData(key) as RandomMinionData;
        if (skillBuff != null) // 풀링 재사용: 이전 스킬 버프 제거
            activeBuffs.Remove(skillBuff);
        skillBuff = null;
        skillBuffTimer = 0f;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        allyBulletSpec = new AllyBulletSpec();
        attackCount = 0;
        skillTimer = 0;
        base.InitMinion();
    }

    public override void Update()
    {
        base.Update();
        if (UpdateSkillBuff())
            return; // 버프 중엔 쿨타임 멈춤
        if (IsBeingDragged) // 드래그 중엔 스킬 발동 안 함
            return;
        if (skillTimer < SKILL_COOLTIME)
        {
            skillTimer += SkillDeltaTime;
            return;
        }
        skillTimer = 0; // 쿨타임이 차면 적이 없어도 바로 발동 (대기 없음)
        skillBuff = new Buff(StatType.AttackSpeed, SKILL_ATTACK_SPEED_MUL, StatOpType.Multiply);
        skillBuffTimer = SKILL_DURATION;
        AddBuff(skillBuff);
        PlayPowerUpEffect(SKILL_DURATION); // 강화 이펙트 (버프 동안)
        PlaySkillAnim();
    }

    // 스킬 버프 시간 처리 (끝나면 RemoveBuff). 버프 중이면 true
    bool UpdateSkillBuff()
    {
        if (skillBuffTimer <= 0f)
            return false;
        skillBuffTimer -= Time.deltaTime;
        if (skillBuffTimer > 0f)
            return true;
        skillBuffTimer = 0f;
        RemoveBuff(skillBuff);
        StopPowerUpEffect();
        skillBuff = null;
        return false;
    }

    public override void Fire(Vector2 dir)
    {
        if (dir == Vector2.zero)
            return;
        if (!isEvolved || (attackCount + 1) % EVOLVE_SKILL_INTERVAL != 0)
        {
            base.Fire(dir);
            if (attackTimer == 0) // 발사에 성공했을 때만 횟수 증가
                attackCount++;
            return;
        }

        FireCircle(Random.Range(0f, 360f), EVOLVE_SKILL_BULLET_COUNT);
        attackTimer = 0;
        attackCount++;
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;

        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }
}

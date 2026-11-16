using UnityEngine;

// [S] 사냥꾼: 평소엔 일반 탄, snipeInterval(5)회 공격마다 저격탄 (스킬 버프 중엔 저격탄 없음, 그동안의 공격은 저격 횟수에 안 셈)
// 저격탄 = 관통(감쇠 없음) + 맞은 적 스턴(엘리트/보스는 절반) + 확정 치명타
// - 한 번의 공격에 탄이 여러 발(다중 타겟 타일 등)이면 그 공격의 탄 전부가 저격탄
// - 연발 타일은 같은 spec을 다시 쓰므로 저격탄의 연발도 저격탄
// 프리팹 탄: 저격탄 = Amulet 프리팹(관통 탄 PierceBulletObject + 부적 그림이 빙글빙글 도는 BulletSpin)
// 스킬(2026-11-15): 공격할 때마다 쿨타임이 차서 SKILL_ATTACK_COUNT회면 SKILL_DURATION초 동안 공속 × SKILL_ATTACK_SPEED_MUL + 타겟 SKILL_SHOOT_COUNT명 (버프 중엔 일반 탄만)
// [각성] (진화) EVOLVE_SNIPE_INTERVAL회 공격마다 저격탄
public class HunterMinion : MinionGrowth1
{
    public const int EVOLVE_SNIPE_INTERVAL = 2;
    public const string SNIPE_BULLET_KEY = "Amulet"; // 저격탄 프리팹 (AddressableResources/BulletObject/Level1/Amulet, 관통 탄 동작 + 부적 그림 회전)
    public const int SKILL_ATTACK_COUNT = 15;
    public const float SKILL_DURATION = 4f;
    public const float SKILL_ATTACK_SPEED_MUL = 3f;
    public const int SKILL_SHOOT_COUNT = 3;
    int skillCharge; // 스킬까지 쌓인 공격 수
    // 스킬 공속 버프 (AddBuff로 걸고 skillBuffTimer가 끝나면 RemoveBuff)
    Buff skillBuff;
    float skillBuffTimer;
    bool IsSkillBuffActive => skillBuffTimer > 0f;

    public override float SkillProgress => IsSkillBuffActive ? 0f : Mathf.Clamp01((float)skillCharge / SKILL_ATTACK_COUNT);
    public override void FillSkillForTest() { skillCharge = SKILL_ATTACK_COUNT - 1; }
    public override int ShootCount => IsSkillBuffActive ? SKILL_SHOOT_COUNT : base.ShootCount;
    HunterMinionData hunterMinionData;
    AllyBulletSpec normalSpec;
    PierceBulletSpec snipeSpec;

    public int snipeInterval;
    public float snipeStunDuration;

    int attackCount;
    bool isSnipeTurn;

    public override void Spawn(Vector2 pos, int lv)
    {
        hunterMinionData = MinionManager.Instance.GetMinionData(key) as HunterMinionData;
        if (skillBuff != null) // 풀링 재사용: 이전 스킬 버프 제거
            activeBuffs.Remove(skillBuff);
        skillBuff = null;
        skillBuffTimer = 0f;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        normalSpec = new AllyBulletSpec();
        snipeSpec = new PierceBulletSpec();
        snipeSpec.key = SNIPE_BULLET_KEY; // 저격탄은 부적(Amulet.png)을 던짐
        snipeInterval = hunterMinionData.snipeInterval;
        snipeStunDuration = hunterMinionData.snipeStunDuration;
        attackCount = 0;
        skillCharge = 0;
        base.InitMinion();
    }

    public override void Update()
    {
        base.Update();
        UpdateSkillBuff();
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

    protected override void OnEvolve()
    {
        snipeInterval = Mathf.Min(snipeInterval, EVOLVE_SNIPE_INTERVAL);
    }

    public override void Fire(Vector2 dir)
    {
        if (dir == Vector2.zero)
            return;

        bool buffActive = IsSkillBuffActive; // 스킬 버프 중엔 저격탄 없음 + 저격 횟수도 안 셈
        isSnipeTurn = !buffActive && (attackCount + 1) % Mathf.Max(1, snipeInterval) == 0;
        base.Fire(dir);
        if (attackTimer == 0) // 발사에 성공했을 때만 횟수 증가
        {
            if (!buffActive)
                attackCount++;
            ChargeSkill();
        }
        isSnipeTurn = false;
    }

    // 버프 중엔 쌓지 않음. 다 차면 공속·멀티 타겟 버프
    void ChargeSkill()
    {
        if (IsSkillBuffActive)
            return;
        skillCharge++;
        if (skillCharge < SKILL_ATTACK_COUNT)
            return;
        skillCharge = 0;
        skillBuff = new Buff(StatType.AttackSpeed, SKILL_ATTACK_SPEED_MUL, StatOpType.Multiply);
        skillBuffTimer = SKILL_DURATION;
        AddBuff(skillBuff);
        PlayPowerUpEffect(SKILL_DURATION); // 강화 이펙트 (버프 동안)
        PlaySkillAnim();
    }

    public override AllyBulletObject GetBullet()
    {
        if (!isSnipeTurn)
        {
            normalSpec.damage = AttackPower;
            normalSpec.mustCrit = false;
            normalSpec.StartBulletSpec();
            return normalSpec.Instantiate(this);
        }

        snipeSpec.damage = AttackPower;
        snipeSpec.pierceCount = hunterMinionData.snipePierceCount;
        snipeSpec.multi = 1f; // 관통해도 피해 감소 없음
        snipeSpec.mustCrit = true;
        snipeSpec.StartBulletSpec();
        snipeSpec.AddBulletBehaviour(new StunOnHitBehavior(snipeStunDuration));
        return snipeSpec.Instantiate(this);
    }
}

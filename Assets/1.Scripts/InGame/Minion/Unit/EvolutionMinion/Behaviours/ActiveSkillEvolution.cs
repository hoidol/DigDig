using UnityEngine;

// (2026-11-26) 머리 위 버튼으로 직접 쓰는 진화 스킬 공용 (눈송이·빙빙이·늑돌이)
// 쿨타임(Cooldown)이 차면 버튼이 빛남 → 누르면 TryStart (2026-11-26 적이 없어도 발동, 눈송이만 경로가 없으면 false) → Duration초 동안 RunTick → End
// 쿨타임은 발동이 끝난 뒤부터 다시 참 (스킬 충전 속도·드래그 중 멈춤 = Minion.SkillDeltaTime)
public abstract class ActiveSkillEvolution : MinionEvolution
{
    protected abstract float Cooldown { get; }
    protected abstract float Duration { get; }

    float cooldownTimer;
    protected float runTimer;

    public override bool HasActiveSkill => true;
    public override float ActiveSkillProgress => Cooldown > 0f ? Mathf.Clamp01(cooldownTimer / Cooldown) : 1f;
    public override bool IsActiveSkillRunning => runTimer > 0f;

    public override bool UseActiveSkill()
    {
        if (!IsActiveSkillReady || !CanAct)
            return false;
        if (!TryStart())
            return false;
        cooldownTimer = 0f;
        runTimer = Mathf.Max(0.01f, Duration);
        minion.PlaySkillAnim();
        PlayCastEffect();
        return true;
    }

    // (2026-11-26) 발동 연출 (임시, 공용 이펙트 재사용): 미니언 자리에 종 색 원 두 개가 퍼짐 + 반짝
    // 색 = SkillColor (종마다 재정의), 더 꾸미려면 PlayCastEffect 재정의
    protected virtual Color SkillColor => new Color(1f, 0.9f, 0.4f);
    protected virtual void PlayCastEffect()
    {
        Vector2 pos = minion.transform.position;
        HeroSkillVisual.PlayRing(pos, 0.7f, SkillColor, 0.3f, 0.1f);
        HeroSkillVisual.PlayRing(pos, 1.3f, SkillColor, 0.5f, 0.06f);
        if (EffectManager.Instance != null)
            EffectManager.Instance.Play(EffectType.Hit, pos);
    }

    protected virtual void Update()
    {
        if (minion == null)
            return;
        if (runTimer > 0f)
        {
            runTimer -= Time.deltaTime;
            RunTick(Time.deltaTime);
            if (runTimer <= 0f)
            {
                runTimer = 0f;
                End();
            }
            return;
        }
        if (cooldownTimer < Cooldown)
            cooldownTimer += minion.SkillDeltaTime;
    }

    public override void OnUnequip()
    {
        if (runTimer > 0f)
        {
            runTimer = 0f;
            End();
        }
    }

    // 테스트·도구용: 쿨타임 바로 채움
    public void FillCooldownForTest() => cooldownTimer = Cooldown;

    protected abstract bool TryStart();
    protected virtual void RunTick(float dt) { }
    protected virtual void End() { }
}

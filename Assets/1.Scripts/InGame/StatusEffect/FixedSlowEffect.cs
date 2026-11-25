using UnityEngine;

// (2026-11-25) 고정 비율 둔화: 이동속도 × slowRate (0.5 = 50% 감소), 중첩 없음 (빙결과 같은 규칙: 강한 배율·긴 시간 유지)
// 빙결과 달리 얼음 표시 없음 → 발밑 둔화 표시 (EffectKey "Slow" = SlowFeetView, 슬로우 중첩 SlowEffect와 같은 표시)
// 기사 칼날 회오리 등
public class FixedSlowEffect : StatusEffect
{
    public override string EffectKey => SlowEffect.EFFECT_KEY;
    public float slowRate;

    public FixedSlowEffect(float duration, float slowRate)
    {
        this.duration = duration;
        this.remainingTimer = duration;
        this.slowRate = slowRate;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        if (effect != this && effect is FixedSlowEffect other)
        {
            slowRate = Mathf.Min(slowRate, other.slowRate);
            remainingTimer = Mathf.Max(remainingTimer, other.duration);
            duration = Mathf.Max(duration, other.duration);
        }
        handler.FixedSlowRate = slowRate;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        handler.FixedSlowRate = 1f;
    }
}

public class SlowEffect : StatusEffect
{
    private float slowRate;  // 0.5 = 50% 감속

    public SlowEffect(float duration, float slowRate)
    {
        this.duration = duration;
        this.remainingTimer = duration;
        this.slowRate = slowRate;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        if (effect != this)
        {
            if (remainingTimer < effect.duration)
            {
                remainingTimer = effect.duration;
            }
            SlowEffect slowEffect = effect as SlowEffect;
            if (slowRate > slowEffect.slowRate)
                slowRate = slowEffect.slowRate;
        }
        //대상 적용
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        //대상 해제
    }
}
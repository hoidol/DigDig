public class SlowEffect : StatusEffect
{
    const float RATE = 0.6f; // 이동속도 60%로 고정

    public SlowEffect(float duration)
    {
        this.duration = duration;
        this.remainingTimer = duration;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        if (effect != this && remainingTimer < effect.duration)
            remainingTimer = effect.duration;

        handler.SlowRate = RATE;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        handler.SlowRate = 1f;
    }
}

public class IceEffect : StatusEffect
{
    public override string EffectKey => "Ice";

    const float RATE = 0.5f; // 이동속도 60%로 고정

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        if (effect != this && remainingTimer < effect.duration)
            remainingTimer = effect.duration;

        handler.SlowRate *= RATE;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        handler.SlowRate /= RATE;
    }

    public IceEffect(float duration)
    {
        this.duration = duration;
        this.remainingTimer = duration;
    }

}

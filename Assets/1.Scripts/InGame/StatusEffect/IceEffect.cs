public class IceEffect : StatusEffect
{
    public override string EffectKey => "Ice";

    public IceEffect(float duration)
    {
        this.duration      = duration;
        this.remainingTimer = duration;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        handler.IsStunned = true;

        if (effect != this)
        {
            if (remainingTimer < effect.duration)
                remainingTimer = effect.duration;
        }
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        handler.IsStunned = false;
    }
}

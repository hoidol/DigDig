public class FreezeEffect : StatusEffect
{
    public override string EffectKey => "Freeze";

    public FreezeEffect(float duration)
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

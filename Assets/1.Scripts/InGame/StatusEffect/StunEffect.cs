// StunEffect.cs
[System.Serializable]
public class StunEffect : StatusEffect
{
    public override string EffectKey => "Stun";
    public StunEffect(float duration)
    {
        this.duration = duration;
        this.remainingTimer = duration;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        handler.IsStunned = true;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {
        handler.IsStunned = false;
    }
}
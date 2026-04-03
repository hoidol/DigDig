using UnityEngine;
// BurnEffect.cs
public class BurnEffect : StatusEffect
{
    float damagePerSecond;
    float damageTimer;
    IHittable hittable;
    public override string EffectKey => "Burn";

    public BurnEffect(float duration, float dps)
    {
        this.duration = duration;
        this.remainingTimer = duration;
        this.damagePerSecond = dps;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        hittable = handler.GetComponent<IHittable>();
        if (this != effect)
        {
            BurnEffect burnEffect = effect as BurnEffect;
            if (remainingTimer < effect.duration)
            {
                remainingTimer = effect.duration;
            }
            if (damagePerSecond < burnEffect.damagePerSecond)
                damagePerSecond = burnEffect.damagePerSecond;
        }
    }

    public override void OnRemove(StatusEffectHandler handler)
    {

    }

    public override void OnUpdate(StatusEffectHandler handler)
    {
        if (damageTimer >= 1)
        {
            hittable?.TakeDamage(damagePerSecond);
            damageTimer = 0;
        }
        damageTimer += Time.deltaTime;
    }
}
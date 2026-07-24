using UnityEngine;
// FlameEffect.cs
public class FlameEffect : StatusEffect
{
    float damagePerSecond;
    float damageTimer;
    IHittable hittable;
    public override string EffectKey => "Burn";

    public FlameEffect(float duration, float dps)
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
            FlameEffect flameEffect = effect as FlameEffect;
            if (remainingTimer < effect.duration)
            {
                remainingTimer = effect.duration;
            }
            if (damagePerSecond < flameEffect.damagePerSecond)
                damagePerSecond = flameEffect.damagePerSecond;
        }

        damageData.damage = damagePerSecond;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {

    }
    DamageData damageData = new DamageData();

    public override void OnUpdate(StatusEffectHandler handler)
    {
        // Debug.Log("FlameEffect OnUpdate");
        if (damageTimer >= 0.5f)
        {
            // Debug.Log("FlameEffect OnUpdate if (damageTimer >= 0.5f)");
            hittable?.TakeDamage(damageData);
            damageTimer = 0f;
        }
        damageTimer += Time.deltaTime;
    }
}
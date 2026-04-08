using UnityEngine;

// 독 DoT - 0.5초마다 데미지
public class PoisonEffect : StatusEffect
{
    float damagePerTick;
    float tickTimer;
    IHittable hittable;
    DamageData damageData = new DamageData();
    public override string EffectKey => "Poison";

    public PoisonEffect(float duration, float damagePerTick)
    {
        this.duration = duration;
        this.remainingTimer = duration;
        this.damagePerTick = damagePerTick;
    }

    public override void OnApply(StatusEffect effect, StatusEffectHandler handler)
    {
        hittable = handler.GetComponent<IHittable>();
        if (this != effect)
        {
            PoisonEffect other = effect as PoisonEffect;
            if (remainingTimer < effect.duration)
                remainingTimer = effect.duration;
            if (damagePerTick < other.damagePerTick)
                damagePerTick = other.damagePerTick;
        }
        damageData.damage = damagePerTick;
    }

    public override void OnRemove(StatusEffectHandler handler) { }

    public override void OnUpdate(StatusEffectHandler handler)
    {
        tickTimer += Time.deltaTime;
        if (tickTimer >= 0.5f)
        {
            hittable?.TakeDamage(damageData);
            tickTimer = 0;
        }
    }
}

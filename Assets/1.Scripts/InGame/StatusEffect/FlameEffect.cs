using UnityEngine;
// FlameEffect.cs
public class FlameEffect : StatusEffect
{
    float damagePerSecond;
    public float DamagePerSecond => damagePerSecond;
    float damageTimer;
    IHittable hittable;
    public override string EffectKey => "Flame";
    DamageData damageData = new DamageData();

    public IAllyUnit Source => damageData.source;

    // source = 화상을 건 아군 (피해 집계용, 없으면 null)
    public FlameEffect(float duration, float dps, IAllyUnit source = null)
    {
        this.duration = duration;
        this.remainingTimer = duration;
        this.damagePerSecond = dps;

        damageData.attackType = AttackType.Magic;
        damageData.source = source;
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
            {
                damagePerSecond = flameEffect.damagePerSecond;
                damageData.source = flameEffect.Source; // 더 센 화상으로 바뀌면 피해 출처도 그쪽
            }
        }

        damageData.damage = damagePerSecond;
    }

    public override void OnRemove(StatusEffectHandler handler)
    {

    }

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
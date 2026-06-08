using UnityEngine;

public interface IHittable
{
    Transform Transform
    {
        get;
    }
    void TakeDamage(DamageData damageData);
    bool CanHit();

    void ApplyStatusEffect(StatusEffect effect);
}
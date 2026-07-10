using UnityEngine;

public interface IHittable
{
    Transform Transform
    {
        get;
    }
    float MaxHp
    {
        get;
    }
    float CurHp
    {
        get;
    }
    void TakeDamage(DamageData damageData);
    bool CanHit();

    void ApplyStatusEffect(StatusEffect effect);
}
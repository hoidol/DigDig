using UnityEngine;

public abstract class FireEnhanceAbility : SlimeEnhanceAbility, IFireSlimeEnhanceAbility
{
    public abstract void Fire(Vector2 dir);
}
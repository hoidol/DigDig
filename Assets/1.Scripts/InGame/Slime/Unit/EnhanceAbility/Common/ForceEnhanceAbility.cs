using UnityEngine;

public abstract class ForceEnhanceAbility : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public abstract void Force(Slime slime);
}
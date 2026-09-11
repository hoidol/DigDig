using UnityEngine;

public class WackyEnhance6Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    public void Force(Slime slime)
    {
        (slime as WackySlime).wackyFireSpeed = 1.1f;       
    }
}
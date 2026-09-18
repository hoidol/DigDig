using UnityEngine;

public class BounceEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 튕기는 횟수(bounce) +1
    public void Force(Slime slime)
    {
        (slime as BounceSlime).bounceCount++;
    }
}
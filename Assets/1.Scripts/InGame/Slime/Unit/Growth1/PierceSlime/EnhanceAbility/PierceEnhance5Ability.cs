using UnityEngine;

public class PierceEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 관통 횟수 +1
    public void Force(Slime slime)
    {
        (slime as PierceSlime).pierceCount++;
    }
}

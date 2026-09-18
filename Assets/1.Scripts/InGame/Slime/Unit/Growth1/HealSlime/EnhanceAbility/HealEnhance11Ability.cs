using UnityEngine;

public class HealEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
     // 회복량(healAmounts) +1 증가
    public void Force(Slime slime)
    {
        (slime as HealSlime).healAmount += 1f;
    }
}

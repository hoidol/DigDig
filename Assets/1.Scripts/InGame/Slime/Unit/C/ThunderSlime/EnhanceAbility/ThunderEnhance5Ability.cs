using UnityEngine;

public class ThunderEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 낙뢰 범위 15% 증가
    public void Force(Slime slime)
    {
        (slime as ThunderSlime).strikeRadius *= 1.15f;
    }
}

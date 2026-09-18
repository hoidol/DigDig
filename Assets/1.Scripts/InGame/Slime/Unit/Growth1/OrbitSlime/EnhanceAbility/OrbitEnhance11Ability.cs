using UnityEngine;

public class OrbitEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 공전 반지름(radius) 20% 증가
    public void Force(Slime slime)
    {
        (slime as OrbitSlime).radius *= 1.2f;
    }
}
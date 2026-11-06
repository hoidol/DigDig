using UnityEngine;

public class OrbitEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 공전 회전 속도(rotationSpeed) 10% 증가
    public void Force(Slime slime)
    {
        (slime as OrbitSlime).rotationSpeed *= 1.1f;
    }
}
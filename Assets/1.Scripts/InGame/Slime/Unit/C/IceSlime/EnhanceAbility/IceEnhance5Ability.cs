using UnityEngine;

public class IceEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
   // 빙결(둔화) 지속시간 20% 증가
    public void Force(Slime slime)
    {
        (slime as IceSlime).iceDuration *= 1.2f;
    }
}
using UnityEngine;

public class BoomEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
     // 폭발 범위(boomRange) 15% 증가
    public void Force(Slime slime)
    {
        (slime as BoomSlime).boomRange *= 1.15f;
    }
}
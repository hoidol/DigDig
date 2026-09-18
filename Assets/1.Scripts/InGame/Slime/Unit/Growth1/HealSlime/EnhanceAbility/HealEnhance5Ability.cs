using UnityEngine;

public class HealEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
     // 회복 쿨타임(healCooltime) 5% 감소
    public void Force(Slime slime)
    {
        (slime as HealSlime).healCoolTime *= 0.95f;
    }
}
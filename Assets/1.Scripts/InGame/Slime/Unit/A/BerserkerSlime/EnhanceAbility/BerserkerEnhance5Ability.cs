using UnityEngine;

public class BerserkerEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 최대 중첩 10 > 13
    public int maxStack = 13;
    public void Force(Slime slime)
    {
        BerserkerSlime berserker = slime as BerserkerSlime;
        berserker.maxStack = Mathf.Max(berserker.maxStack, maxStack);
    }
}

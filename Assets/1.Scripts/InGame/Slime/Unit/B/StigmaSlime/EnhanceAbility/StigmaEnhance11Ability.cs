using UnityEngine;

public class StigmaEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // [각성] 낙인 최대 중첩 3 > 5
    public int maxStack = 5;
    public void Force(Slime slime)
    {
        StigmaSlime stigma = slime as StigmaSlime;
        stigma.maxStack = Mathf.Max(stigma.maxStack, maxStack);
    }
}

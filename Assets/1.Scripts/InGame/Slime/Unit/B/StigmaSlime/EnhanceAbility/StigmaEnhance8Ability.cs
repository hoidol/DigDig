using UnityEngine;

public class StigmaEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 낙인 중첩당 추가 피해 5% > 7%
    public float damagePerStack = 0.07f;
    public void Force(Slime slime)
    {
        StigmaSlime stigma = slime as StigmaSlime;
        stigma.damagePerStack = Mathf.Max(stigma.damagePerStack, damagePerStack);
    }
}

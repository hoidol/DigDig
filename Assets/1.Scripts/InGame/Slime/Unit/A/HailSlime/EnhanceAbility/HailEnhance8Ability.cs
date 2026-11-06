using UnityEngine;

public class HailEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // [각성] 눈 2개
    public int hailCount = 2;
    public void Force(Slime slime)
    {
        HailSlime hail = slime as HailSlime;
        hail.hailCount = Mathf.Max(hail.hailCount, hailCount);
    }
}

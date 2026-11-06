using UnityEngine;

public class DopamineEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // [각성] 추가 발사 1발 > 2발
    public int extraShotCount = 2;
    public void Force(Slime slime)
    {
        DopamineSlime dopamine = slime as DopamineSlime;
        dopamine.extraShotCount = Mathf.Max(dopamine.extraShotCount, extraShotCount);
    }
}

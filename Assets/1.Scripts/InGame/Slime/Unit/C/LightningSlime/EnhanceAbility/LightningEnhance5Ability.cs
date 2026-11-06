using UnityEngine;

public class LightningEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 낙뢰 탐색 범위(searchRadius) 20% 증가
    public void Force(Slime slime)
    {
        (slime as LightningSlime).searchRadius*= 1.2f;
    }
}

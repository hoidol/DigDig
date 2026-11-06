using UnityEngine;

public class LightningEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
     // 체인 횟수 +1
    public void Force(Slime slime)
    {
        (slime as LightningSlime).lightningCount +=1;
    }
}

using UnityEngine;

public class ThunderEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 낙뢰 최대 타격 수 3개 증가
    public int addHitCount = 3;
    public void Force(Slime slime)
    {
        ThunderSlime thunder = slime as ThunderSlime;
        thunder.maxHitCount += addHitCount;
            
    }
}

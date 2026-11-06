using UnityEngine;

public class IronEnhance8Ability : SlimeEnhanceAbility,IForceSlimeEnhanceAbility
{
    
    // 엘리트, 보스 데미지 배율 +0.5
    public float addMul = 0.2f;
    public void Force(Slime slime)
    {
        (slime as IronSlime).eliteBossDamageMul += addMul;
    }
}

using UnityEngine;

public class WackyEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 추가 발사 쿨타임 10% 감소
    public void Force(Slime slime)
    {
        (slime as WackySlime).wackyFireSpeed = 1.1f;       
    }
}
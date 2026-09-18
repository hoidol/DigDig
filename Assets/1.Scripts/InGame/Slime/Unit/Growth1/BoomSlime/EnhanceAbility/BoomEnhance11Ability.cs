using UnityEngine;

public class BoomEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 폭발 시 최대 타격 개수 증가
    //hitCount = 5 -> 8
    public void Force(Slime slime)
    {
        (slime as BoomSlime).maxHitCount = 8;
    }
}
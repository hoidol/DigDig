using UnityEngine;

public class MortarEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // [각성] 폭발에 맞은 적 슬로우 확률 25% → 50%
    public float slowChance = 0.5f;

    public void Force(Slime slime)
    {
        (slime as MortarSlime).slowChance = slowChance;
    }
}

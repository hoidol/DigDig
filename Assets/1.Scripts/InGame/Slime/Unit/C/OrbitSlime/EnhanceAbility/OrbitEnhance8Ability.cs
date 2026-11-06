using UnityEngine;

public class OrbitEnhance8Ability : SlimeEnhanceAbility
{
    // [각성] 회전에 닿은 적 3% 확률로 0.1초 스턴 (엘리트/보스는 0.05초)
    public float stunChance = 0.03f;
    public float stunDuration = 0.1f;
}

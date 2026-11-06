using UnityEngine;

public class ShieldEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // [각성] 가시방패: 막혀 있는 적에게 초당 최대 체력의 1% 피해 (ShieldOrbitOrb)
    public float thornMaxHpRatio = 0.01f;
    public void Force(Slime slime)
    {
        ShieldSlime shield = slime as ShieldSlime;
        shield.thornMaxHpRatio = Mathf.Max(shield.thornMaxHpRatio, thornMaxHpRatio);
    }
}

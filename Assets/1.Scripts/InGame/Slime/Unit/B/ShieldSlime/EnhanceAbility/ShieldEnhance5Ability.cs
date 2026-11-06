using UnityEngine;

public class ShieldEnhance5Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 방패당 막는 적 2기 > 3기
    public int blockCountPerShield = 3;
    public void Force(Slime slime)
    {
        ShieldSlime shield = slime as ShieldSlime;
        shield.blockCountPerShield = Mathf.Max(shield.blockCountPerShield, blockCountPerShield);
    }
}

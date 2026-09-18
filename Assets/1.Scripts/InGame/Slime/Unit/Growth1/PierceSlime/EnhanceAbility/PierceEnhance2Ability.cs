using UnityEngine;

public class PierceEnhance2Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 공격력 7% 증가
    public void Force(Slime slime)
    {
        Buff attackPowerBuff = new Buff(StatType.AttackPower, 1.07f, StatOpType.Multiply);
        slime.AddBuff(attackPowerBuff);
        
    }
}

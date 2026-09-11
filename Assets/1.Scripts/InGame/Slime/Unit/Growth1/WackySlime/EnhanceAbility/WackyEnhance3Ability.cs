using UnityEngine;

public class WackyEnhance3Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    //공격력 10% 증가 
    Buff attackPowerBuff;
    public void Force(Slime slime)
    {
        attackPowerBuff = new Buff(StatType.AttackPower, 1.1f, StatOpType.Multiply);
        slime.AddBuff(attackPowerBuff);
    }
}
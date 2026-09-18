using UnityEngine;

public class WackyEnhance14Ability : SlimeEnhanceAbility,IForceSlimeEnhanceAbility
{
    //공격력 10% 증가 + 공격속도 10% 증가

    Buff attackPowerBuff;
    Buff attackSpeedBuff;
    public void Force(Slime slime)
    {
        attackPowerBuff = new Buff(StatType.AttackPower, 1.1f, StatOpType.Multiply);
        slime.AddBuff(attackPowerBuff);
        attackSpeedBuff = new Buff(StatType.AttackSpeed, 1.1f, StatOpType.Multiply);
        slime.AddBuff(attackSpeedBuff);
    }
}
using UnityEngine;

public class FlameEnhance14Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    //공격력 10% 증가, 공격 속도 10% 증가
    public void Force(Slime slime)
    {
        Buff attackPowerBuff = new Buff(StatType.AttackPower, 1.1f, StatOpType.Multiply);
        slime.AddBuff(attackPowerBuff);
        
        Buff attackSpeedBuff = new Buff(StatType.AttackSpeed, 1.1f, StatOpType.Multiply);
        slime.AddBuff(attackSpeedBuff);
    }
}

using UnityEngine;

public class BoomEnhance14Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
     // 공격력 +3 증가
    public void Force(Slime slime)
    {
        Buff attackPowerBuff = new Buff(StatType.AttackPower, 3f, StatOpType.Add);
        slime.AddBuff(attackPowerBuff);
    }
}
using UnityEngine;

public class BoomEnhance2Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
     // 공격력 +2 증가
    public void Force(Slime slime)
    {
        Buff attackPowerBuff = new Buff(StatType.AttackPower, 2f, StatOpType.Add);
        slime.AddBuff(attackPowerBuff);
    }
}
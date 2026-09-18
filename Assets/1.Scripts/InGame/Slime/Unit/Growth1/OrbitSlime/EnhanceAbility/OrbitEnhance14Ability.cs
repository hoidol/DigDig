using UnityEngine;

public class OrbitEnhance14Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    //공격력 10% 증가, 회전 속도 10% 증가
    public void Force(Slime slime)
    {
        Buff attackPowerBuff = new Buff(StatType.AttackPower, 1.1f, StatOpType.Multiply);
        slime.AddBuff(attackPowerBuff);
        
        (slime as OrbitSlime).rotationSpeed *= 1.1f;
    }
}

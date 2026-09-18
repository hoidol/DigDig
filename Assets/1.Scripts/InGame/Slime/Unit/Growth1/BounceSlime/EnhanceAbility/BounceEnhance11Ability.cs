using UnityEngine;

public class BounceEnhance11Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // 발사 속도 10% 증가
    
    public void Force(Slime slime)
    {
        Buff attackSpeedBuff = new Buff(StatType.AttackSpeed, 1.1f, StatOpType.Multiply);
        slime.AddBuff(attackSpeedBuff);
    }
}

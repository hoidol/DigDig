using UnityEngine;

public class LightningEnhance14Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    //공격 속도 +10% 증가 
    public void Force(Slime slime)
    {
        Buff attckSpeedBuff = new Buff(StatType.AttackSpeed, 1.1f, StatOpType.Multiply);
        slime.AddBuff(attckSpeedBuff);
    }
}
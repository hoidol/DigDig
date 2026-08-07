using UnityEngine;
//체력 +25, 공격력 +8 튕김 효율 50% > 60% > 70% 
//힘, 자연
public class MountainItem : Item
{
    //공격력 +8
    float[] maxHps = { 25, 50, 75 };
    float[] attackPowers = { 8, 11, 15 };
    float[] recoveryHps = { 0.2f, 0.3f, 0.4f };
    Buff maxHpBuff;
    Buff attackPowerBuff;
    Buff recoveryHpBuff;
    public override void OnUnequip()
    {
        base.OnUnequip();
        Release();

    }
    void Release()
    {
        if (maxHpBuff != null)
            Character.Instance.RemoveBuff(maxHpBuff);

        if (attackPowerBuff != null)
            Character.Instance.RemoveBuff(attackPowerBuff);

        if (attackPowerBuff != null)
            Character.Instance.RemoveBuff(attackPowerBuff);

    }
    public override void UpdateItem()
    {
        base.UpdateItem();
        Release();
        //최대 체력
        maxHpBuff = new Buff(StatType.MaxHp, maxHps[count - 1], StatOpType.Add);
        Character.Instance.AddBuff(maxHpBuff);

        //공격 효율
        attackPowerBuff = new Buff(StatType.AttackPower, attackPowers[count - 1], StatOpType.Add);
        Character.Instance.AddBuff(attackPowerBuff);

        //초당 회복력
        recoveryHpBuff = new Buff(StatType.RecoveryHp, recoveryHps[count - 1], StatOpType.Add);
        Character.Instance.AddBuff(recoveryHpBuff);
  
    }


    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"체력 +{maxHps[lv - 1]} 공격력 +{attackPowers[lv - 1]} 초당 회복력 +{recoveryHps[lv - 1]}";
    }
}
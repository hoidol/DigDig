using UnityEngine;
//체력 +25, 공격력 +8 
//힘, 자연
public class MountainItem : Item
{
    //공격력 +8
    float maxHp = 20;
    float attackPower = 8;
    // float recoveryHp = 0.3f;
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
        maxHpBuff = new Buff(StatType.MaxHp, maxHp *count, StatOpType.Add);
        Character.Instance.AddBuff(maxHpBuff);

        //공격 효율
        attackPowerBuff = new Buff(StatType.AttackPower, attackPower*count, StatOpType.Add);
        Character.Instance.AddBuff(attackPowerBuff);

        //초당 회복력
        // recoveryHpBuff = new Buff(StatType.RecoveryHp, recoveryHp*count, StatOpType.Add);
        // Character.Instance.AddBuff(recoveryHpBuff);
  
    }


    public override string GetDescription()
    {
        return $"체력 +{maxHp} 공격력 +{attackPower}";
        //return string.Format(TranslateManager.GetText($"{key}_Desc"),maxHp,attackPower);
    }
}
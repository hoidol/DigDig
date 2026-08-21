using UnityEngine;

public class ForceItem : Item
{
    //공격력 4증가, 공격속도 3 증가    
    float attackPower = 4;
    float attackSpeed = 3;
    Buff atkPowerBuff;
    Buff atkSpeedBuff;
    public override void UpdateItem()
    {
        base.UpdateItem();
        Release();
        float addAP =  count *attackPower;
        //공격력
        atkPowerBuff = new Buff(StatType.AttackPower, addAP, StatOpType.Add);
        Character.Instance.AddBuff(atkPowerBuff);

        float addAS =  count *attackSpeed;
        //공격력
        atkSpeedBuff = new Buff(StatType.AttackSpeed, addAS, StatOpType.Add);
        Character.Instance.AddBuff(atkSpeedBuff);
    }
    void Release()
    {
        if (atkPowerBuff != null)
            Character.Instance.RemoveBuff(atkPowerBuff);
        if (atkSpeedBuff != null)
            Character.Instance.RemoveBuff(atkSpeedBuff);
    }


    public override string GetDescription()
    {
        return $"공격력 +{attackPower} 공격 속도 +{attackSpeed}";
        //return string.Format(TranslateManager.GetText("{key}_Desc"),attackPower,attackSpeed);
    }
}
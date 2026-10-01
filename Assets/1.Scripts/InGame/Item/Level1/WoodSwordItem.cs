using UnityEngine;

public class WoodSwordItem : Item
{
    //공격력 4증가, 
    float attackPower = 4;
    Buff atkPowerBuff;
    public override void UpdateItem()
    {
        base.UpdateItem();
        Release();
        float addAP =  count *attackPower;
        //공격력
        atkPowerBuff = new Buff(StatType.AttackPower, addAP, StatOpType.Add);
        Character.Instance.AddBuff(atkPowerBuff);

    }
    void Release()
    {
        if (atkPowerBuff != null)
            Character.Instance.RemoveBuff(atkPowerBuff);
    }


    public override string GetDescription()
    {
        return $"공격력 +{attackPower}";
        //return string.Format(TranslateManager.GetText("{key}_Desc"),attackPower,attackSpeed);
    }
}
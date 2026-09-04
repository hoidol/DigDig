using UnityEngine;

public class CriticalItem : Item
{
    //크리티컬 확률 증가
    float critChance = 10;
    Buff critChanceBuff;
    public override void UpdateItem()
    {
        base.UpdateItem();
        Release();
        float addCritChance =  critChance;
        //크리티컬 확률
        critChanceBuff = new Buff(StatType.CritChance, addCritChance, StatOpType.Add);
        Character.Instance.AddBuff(critChanceBuff);

    }
    void Release()
    {
        if (critChanceBuff != null)
            Character.Instance.RemoveBuff(critChanceBuff);
    }


    public override string GetDescription()
    {
        return $"크리티컬 확률 +{critChance}%";
        //return string.Format(TranslateManager.GetText("{key}_Desc"),critChance);
    }
}
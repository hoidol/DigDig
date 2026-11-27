using UnityEngine;

public class SkullCaneRune : Rune
{
    //크리티컬 확률 증가
    float critChance = 10;
    Buff critChanceBuff;
    public override void UpdateRune()
    {
        base.UpdateRune();
        Release();
        float addCritChance =  critChance;
        //크리티컬 확률
        critChanceBuff = new Buff(StatType.CritChance, addCritChance, StatOpType.Add);
        Slime.Instance.AddBuff(critChanceBuff);

    }
    void Release()
    {
        if (critChanceBuff != null)
            Slime.Instance.RemoveBuff(critChanceBuff);
    }


    public override string GetDescription()
    {
        return $"크리티컬 확률 +{critChance}%";
        //return string.Format(TranslateManager.GetText("{key}_Desc"),critChance);
    }
}
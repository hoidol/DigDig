using UnityEngine;

public class ForceItem : Item 
{
    //공격력 +5, 탄 효율 50%
    float[] attackPowers ={5,8,11};
    float[] ammoEfficiencies ={1.3f,1.6f,2f};
    Buff atkPowerBuff;
    Buff ammoEfficiencyBuff;
    public override void UpdateItem()
    {
        //공격력
        if(atkPowerBuff != null)
            Player.Instance.RemoveBuff(atkPowerBuff);

        atkPowerBuff = new Buff(StatType.AttackPower,attackPowers[count-1],StatOpType.Add);
        Player.Instance.AddBuff(atkPowerBuff);


        //탄 효율
        if(ammoEfficiencyBuff != null)
            Player.Instance.RemoveBuff(ammoEfficiencyBuff);

        ammoEfficiencyBuff = new Buff(StatType.AmmoEfficiency,ammoEfficiencies[count-1],StatOpType.Multiply);
        Player.Instance.AddBuff(ammoEfficiencyBuff);
    }
}
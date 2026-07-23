using UnityEngine;

//체력 +15, 회복 초당 0.5
public class NatureItem : Item {
    
    //공격력 +5, 탄 효율 50%
    float[] maxHps ={15,30,45};
    float[] ammoEfficiencies ={0.5f,0.7f,1f};
    Buff maxHpBuff;
    Buff recoveryHpBuff;
    public override void UpdateItem()
    {
        //공격력
        if(maxHpBuff != null)
            Player.Instance.RemoveBuff(maxHpBuff);

        maxHpBuff = new Buff(StatType.MaxHp,maxHps[count-1],StatOpType.Add);
        Player.Instance.AddBuff(maxHpBuff);


        //탄 효율
        if(recoveryHpBuff != null)
            Player.Instance.RemoveBuff(recoveryHpBuff);

        recoveryHpBuff = new Buff(StatType.RecoveryHp,ammoEfficiencies[count-1],StatOpType.Add);
        Player.Instance.AddBuff(recoveryHpBuff);
    }


    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"체력 +{maxHps[lv-1]} 초당 회복력 +{ammoEfficiencies[lv-1]}";
    }
}
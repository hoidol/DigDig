using UnityEngine;

//체력 +15, 회복 초당 0.5
public class NatureItem : Item {
    
    //공격력 +5, 탄 효율 30%
    float[] maxHps ={15,30,45};
    float[] ammoEfficiencies ={0.2f,0.3f,0.4f};
    Buff maxHpBuff;
    Buff recoveryHpBuff;

    public override void OnUnequip()
    {
        base.OnUnequip();
        Release();
     
    }
    public override void UpdateItem()
    {
        Release();
        //체력
        maxHpBuff = new Buff(StatType.MaxHp,maxHps[count-1],StatOpType.Add);
        Player.Instance.AddBuff(maxHpBuff);


        //탄 효율
        recoveryHpBuff = new Buff(StatType.RecoveryHp,ammoEfficiencies[count-1],StatOpType.Add);
        Player.Instance.AddBuff(recoveryHpBuff);
    }


    void Release()
    {
        if(maxHpBuff != null)
            Player.Instance.RemoveBuff(maxHpBuff);

        if(recoveryHpBuff != null)
            Player.Instance.RemoveBuff(recoveryHpBuff);
    }


    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"체력 +{maxHps[lv-1]} 초당 회복력 +{ammoEfficiencies[lv-1]}";
    }
}
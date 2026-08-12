using UnityEngine;

public class ForceItem : Item
{
    //공격력 4증가 
    
    float attackPower = 3;
    float attackSpeed = 10;
    // float[] ammoEfficiencies = { 0.7f, 0.6f, 0.5f };
    Buff atkPowerBuff;
    Buff atkSPeedBuff;
    // Buff ammoEfficiencyBuff;
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
        atkSPeedBuff = new Buff(StatType.AttackSpeed, addAS, StatOpType.Add);
        Character.Instance.AddBuff(atkSPeedBuff);


        //탄 효율
        // ammoEfficiencyBuff = new Buff(StatType.AmmoEfficiency, ammoEfficiencies[count - 1], StatOpType.Multiply);
        // Player.Instance.AddBuff(ammoEfficiencyBuff);
    }
    void Release()
    {

        if (atkPowerBuff != null)
            Character.Instance.RemoveBuff(atkPowerBuff);
        if (atkSPeedBuff != null)
            Character.Instance.RemoveBuff(atkSPeedBuff);

        // if (ammoEfficiencyBuff != null)
        //     Player.Instance.RemoveBuff(ammoEfficiencyBuff);
    }


    public override string GetDescription()
    {
        return $"공격력 +{attackPower} 공격 속도 +{attackSpeed}";
    }
}
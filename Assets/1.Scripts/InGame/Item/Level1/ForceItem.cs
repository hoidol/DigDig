using UnityEngine;

public class ForceItem : Item
{
    //공격력 +5
    float[] attackPowers = { 5, 8, 11 };
    // float[] ammoEfficiencies = { 0.7f, 0.6f, 0.5f };
    Buff atkPowerBuff;
    // Buff ammoEfficiencyBuff;
    public override void UpdateItem()
    {
        base.UpdateItem();
        Release();

        //공격력
        atkPowerBuff = new Buff(StatType.AttackPower, attackPowers[count - 1], StatOpType.Add);
        Character.Instance.AddBuff(atkPowerBuff);


        //탄 효율
        // ammoEfficiencyBuff = new Buff(StatType.AmmoEfficiency, ammoEfficiencies[count - 1], StatOpType.Multiply);
        // Player.Instance.AddBuff(ammoEfficiencyBuff);
    }
    void Release()
    {

        if (atkPowerBuff != null)
            Character.Instance.RemoveBuff(atkPowerBuff);

        // if (ammoEfficiencyBuff != null)
        //     Player.Instance.RemoveBuff(ammoEfficiencyBuff);
    }


    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"공격력 +{attackPowers[lv - 1]}";
    }
}
using UnityEngine;
//체력 +25, 공격력 +8, 튕김 +3 튕김 효율 50% > 60% > 70% 
//힘, 자연
public class MountainItem : Item
{
    //공격력 +5, 탄 효율 50%
    float[] maxHps = { 25, 50, 75 };
    float[] attackPowers = { 8, 11, 15 };
    float[] ammoEfficiencies = { 0.5f, 0.6f, 0.7f };
    int[] bounces = { 3, 4, 5 };
    Buff maxHpBuff;
    Buff attackPowerBuff;
    Buff recoveryHpBuff;
    int bounce;
    public override void OnUnequip()
    {
        base.OnUnequip();
        Release();

    }
    void Release()
    {
        if (maxHpBuff != null)
            Player.Instance.RemoveBuff(maxHpBuff);

        if (attackPowerBuff != null)
            Player.Instance.RemoveBuff(attackPowerBuff);

        if (attackPowerBuff != null)
            Player.Instance.RemoveBuff(attackPowerBuff);

        if (bounce != 0)
            Player.Instance.AddBounce(-bounce);
    }
    public override void UpdateItem()
    {
        base.UpdateItem();
        Release();
        //최대 체력
        maxHpBuff = new Buff(StatType.MaxHp, maxHps[count - 1], StatOpType.Add);
        Player.Instance.AddBuff(maxHpBuff);

        //공격 효율
        attackPowerBuff = new Buff(StatType.AttackPower, attackPowers[count - 1], StatOpType.Add);
        Player.Instance.AddBuff(attackPowerBuff);

        //탄 효율
        recoveryHpBuff = new Buff(StatType.RecoveryHp, ammoEfficiencies[count - 1], StatOpType.Add);
        Player.Instance.AddBuff(recoveryHpBuff);

        //튕김 수 
        bounce = bounces[count - 1];
        Player.Instance.AddBounce(bounce);
    }


    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"체력 +{maxHps[lv - 1]} 공격력 +{attackPowers[lv - 1]} 탄효율 +{ammoEfficiencies[lv - 1]} 튕김 추가 +{bounces[lv - 1]}";
    }
}
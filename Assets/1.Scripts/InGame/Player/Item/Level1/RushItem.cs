using UnityEngine;

//10초마다 3초 동안 발사 속도 60% > 90 > 120% - 아드레날린..
public class RushItem : TriggerCycleItem 
{
    float[] coolTimes = {10,8,6};
    float[] attackSpeeds = {1.6f,1.9f,2.2f};
    float[] durations = {3f,4f,5f};

    Buff attackSpeedBuff;

    public override void UpdateItem()
    {
        base.UpdateItem();
        activeTime = durations[count-1];
        coolTime = coolTimes[count-1];
    }
    
    public override void OnUnequip()
    {
        base.OnUnequip();

        if(attackSpeedBuff != null)
            Player.Instance.RemoveBuff(attackSpeedBuff);
            
        attackSpeedBuff = null;
    }

    public override void OnActivate()
    {
        attackSpeedBuff = new Buff( StatType.AttackSpeed,attackSpeeds[count-1], StatOpType.Multiply);
        Player.Instance.AddBuff(attackSpeedBuff);
    }

    public override void OnDeactivate()
    {
        if(attackSpeedBuff != null)
            Player.Instance.RemoveBuff(attackSpeedBuff);

        attackSpeedBuff = null;
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"{coolTimes[lv-1]}초마다 {durations[count-1]}초 동안 공격 속도 +{(int)((attackSpeeds[lv-1]-1)*100)}% 증가";
    }
}
using UnityEngine;

public class LuckItem : TriggerItem
{
    //회피 확률 10% 상승
    float dodge = 5;
    Buff dodgeBuff;
    public override void UpdateItem()
    {
        base.UpdateItem();
        Release();
        float addDodge =  count *dodge;
        //공격력
        dodgeBuff = new Buff(StatType.Dodge, addDodge, StatOpType.Add);
        Character.Instance.AddBuff(dodgeBuff);

    }
    void Release()
    {
        if (dodgeBuff != null)
            Character.Instance.RemoveBuff(dodgeBuff);
    }


    public override string GetDescription()
    {
        return $"피해률 +{dodge}%";
    }
}
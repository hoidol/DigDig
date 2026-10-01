// 붕대: 30초마다 체력 5 회복
using UnityEngine;

public class BandageItem : TriggerItem
{
    float healAmount = 5;
    public override void OnEquip()
    {
        coolTime = 30f;
        base.OnEquip();
    }

    public override void OnTrigger()
    {
        base.OnTrigger();
        Debug.Log("BandageItem OnTrigger()");
        Character.Instance.AddHp(healAmount * count);
    }

    public override string GetDescription()
    {
        return $"{coolTime}초 마다 5만큼 체력 회복";
    }
}

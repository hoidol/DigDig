// 칼집: 50초마다 피해 1회 차단
using UnityEngine;

public class SheathItem : TriggerItem
{
    StatusEffectHandler shieldHandler;
    public GameObject effect;

    float[] coolTimes = {50,45,40};

    public override void OnEquip()
    {
        shieldHandler = Character.Instance.GetComponent<StatusEffectHandler>();
        transform.parent = Character.Instance.bodyCenterTr;
        transform.position = Character.Instance.bodyCenterTr.position;
        effect.SetActive(false);
        base.OnEquip();
    }
    public override void UpdateItem()
    {
        coolTime = coolTimes[count-1];
    }

    public override void OnTrigger()
    {
        base.OnTrigger();
        if (shieldHandler.IsShielded) return;
        effect.SetActive(true);
        shieldHandler.Apply(new ShieldEffect(() =>
        {
            effect.SetActive(false);
        }));
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"{coolTimes[lv-1]}초마다 피해 1회 차단";
    }
}

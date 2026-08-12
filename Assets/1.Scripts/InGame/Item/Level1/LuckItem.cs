using UnityEngine;

public class LuckItem : TriggerItem
{
    //1회 방어 40초마다
    StatusEffectHandler shieldHandler;
    public GameObject effect;
    float baseCoolTime = 40;
    float addReduceTime = 5;
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
        base.UpdateItem();
        coolTime = baseCoolTime - addReduceTime * (count-1);
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

    public override string GetDescription()
    {
        return $"{baseCoolTime}초마다 피해 1회 차단 (추가 보유 시 쿨타임 -{addReduceTime} 감소))";
    }
}
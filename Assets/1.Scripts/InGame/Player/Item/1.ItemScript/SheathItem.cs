// 칼집: 50초마다 피해 1회 차단
using UnityEngine;

public class SheathItem : TriggerItem
{
    StatusEffectHandler shieldHandler;
    public GameObject effect;

    float[] coolTimes = {50,45,40};

    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        shieldHandler = player.GetComponent<StatusEffectHandler>();
        transform.parent = player.bodyCenterTr;
        transform.position = player.bodyCenterTr.position;
        effect.SetActive(false);
        base.OnEquip(player);
    }
    public override void UpdateItem()
    {
        coolTime = coolTimes[GetLevel()-1];
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

// 칼집: 40초마다 피해 1회 차단
using UnityEngine;

public class SheathItem : TriggerItem
{
    StatusEffectHandler shieldHandler;
    public GameObject effect;


    public override void OnEquip(Player player)
    {
        shieldHandler = player.GetComponent<StatusEffectHandler>();
        transform.parent = player.bodyCenterTr;
        transform.position = player.bodyCenterTr.position;
        effect.SetActive(false);
        coolTime = 50f;
        base.OnEquip(player);
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

    public override string GetDescription(bool detail = false)
    {
        return "50초마다 피해 1회 차단";
    }
}

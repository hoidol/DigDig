// 조개: count에 따라 일정 시간마다 피해 1회 차단 (40초 / 35초 / 30초)
using UnityEngine;

// [조개]
// count에 따라 40/35/30초 쿨타임마다 피해 1회를 차단하는 방어막(ShieldEffect)을 부여.
// 이미 방어막이 활성화된 상태면 중복 적용하지 않음.
// 방어막 소모(TryBlock) 시 effect 오브젝트 비활성화 콜백으로 시각 효과 해제.
public class SheathItem : TriggerItem
{
    static readonly float[] coolTimes = { 40f, 35f, 30f };

    StatusEffectHandler shieldHandler;
    public GameObject effect;

    public override void OnEquip(Player player)
    {
        shieldHandler = player.GetComponent<StatusEffectHandler>();
        transform.parent = player.bodyCenterTr;
        transform.position = player.bodyCenterTr.position;
        effect.SetActive(false);
        UpdateItem();
        base.OnEquip(player);
    }

    public override void UpdateItem()
    {
        coolTime = coolTimes[Mathf.Clamp(count - 1, 0, coolTimes.Length - 1)];
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

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        float ct = coolTimes[Mathf.Clamp(c - 1, 0, coolTimes.Length - 1)];
        return $"{ct}초마다 피해 1회 차단";
    }
}

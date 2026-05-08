using UnityEngine;

// 드론: 360도 공격하는 드론을 소환 (지속시간 10s > 12s > 15s, 공격속도 초당 1발 > 1.5발 > 2.5발)
public class DroneItem : TriggerCycleItem
{
    static readonly float[] activeTimes = { 15f, 20f, 25f };
    public AroundAttackDrone drone;

    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        drone.gameObject.SetActive(false);
        drone.transform.parent = null;
    }
    public override void OnUnequip(Player player)
    {
        drone.gameObject.SetActive(false);
        drone.transform.parent = transform;
        base.OnUnequip(player);

    }
    public override void UpdateItem()
    {
        activeTime = activeTimes[UnityEngine.Mathf.Clamp(count - 1, 0, activeTimes.Length - 1)];
    }

    public override void OnActivate()
    {
        drone.gameObject.SetActive(true);
        drone.Spawn(transform.position, count);

    }

    public override void OnDeactivate()
    {
        drone.gameObject.SetActive(false);
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"{coolTime}초 마다 드론을 소환합니다. {activeTimes[c - 1]}초 동안 공격합니다.";
    }
}

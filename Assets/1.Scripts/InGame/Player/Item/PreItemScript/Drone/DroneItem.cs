using UnityEngine;

// 드론: 15초 동안 공격하는 드론 소환
public class DroneItem : TriggerCycleItem
{
    public AroundAttackDrone drone;

    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        activeTime = 15f;
        drone.gameObject.SetActive(false);
        drone.transform.parent = null;
    }

    public override void OnUnequip(Player player)
    {
        drone.gameObject.SetActive(false);
        drone.transform.parent = transform;
        base.OnUnequip(player);
    }

    public override void OnActivate()
    {
        drone.attackInterval = 0.5f;
        drone.gameObject.SetActive(true);
        drone.Spawn(transform.position, 2);
    }

    public override void OnDeactivate()
    {
        drone.gameObject.SetActive(false);
    }

    public override string GetDescription(bool detail = false)
    {
        return $"{coolTime}초 마다 드론을 소환합니다. 15초 동안 공격합니다.";
    }
}

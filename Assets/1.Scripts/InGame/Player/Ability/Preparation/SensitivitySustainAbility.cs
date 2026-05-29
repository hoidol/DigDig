// 예민함 유지 - 광석 파괴 시 1회 연속 발사
using UnityEngine;

public class SensitivitySustainAbility : Ability
{
    public override string GetDescription(bool detail = false)
    {
        return "광석 제거 후 1회 연속 발사";
    }

    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStone);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStone);
    }

    void OnDestroyedStone(DestroyedStoneEvent e)
    {
        Player.Instance.QueueExtraShot(1);
    }
}

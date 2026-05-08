// 예민함 유지 - 광석 파괴 시 count만큼 추가 발사
using UnityEngine;

public class SensitivitySustainAbility : Ability
{
    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;
        return $"광석 제거 후 {c}회 연속 발사";
    }
    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<OreStoneDestroyedEvent>(OnOreDestroyed);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<OreStoneDestroyedEvent>(OnOreDestroyed);
    }

    void OnOreDestroyed(OreStoneDestroyedEvent e)
    {
        Player.Instance.QueueExtraShot(count);
    }
}

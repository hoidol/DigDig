using UnityEngine;

public class CloverItem : Item
{
    public float bonusChance = 0.3f; // 30%

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
        if (Random.value < bonusChance * count)
            Player.Instance.AddGold(1);
    }

    public override string GetDescription(int c = -1)
    {
        if (c <= 0) c = count;
        return $"광석에서 골드 {bonusChance * c * 100:0}% 확률로 2배";
    }
}

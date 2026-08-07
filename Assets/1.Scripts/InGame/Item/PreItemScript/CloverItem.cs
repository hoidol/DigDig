using UnityEngine;

// 클로버: 광석 파괴 시 30% 확률로 골드 추가 드랍
public class CloverItem : Item
{
    const float CHANCE = 0.30f;

    public override void OnEquip()
    {
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStone);
    }

    public override void OnUnequip()
    {
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStone);
    }

    void OnDestroyedStone(DestroyedStoneEvent e)
    {
        if (Random.value < CHANCE)
            Gold.Dropped(e.stone.transform.position);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"광석 파괴 시 {CHANCE * 100:0}% 확률로 골드 드랍";
    }
}

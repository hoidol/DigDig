using UnityEngine;

public class CloverItem : Item
{
    public float[] bonusChances = { 0.2f, 0.3f, 0.4f }; // 30%

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
        if (Random.value < bonusChances[count - 1])
        {
            Debug.Log("CloverItem Drop Gold");
            Gold.Dropped(e.oreStone.transform.position, "1");
        }

    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"광석 파괴 시 {bonusChances[c - 1] * 100:0}% 확률로 골드 드랍";
    }
}

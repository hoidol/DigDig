using UnityEngine;

// 처치 시 5% 확률로 체력 회복 +2
public class DrainItem : Item
{
    float[] healChances = { 0.05f, 0.08f, 0.12f };
    int[] healAmounts = { 2, 3, 4 };

    void OnEnable()
    {
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    void OnDisable()
    {
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    void OnDestroyedStoneEvent(DestroyedStoneEvent e)
    {
        OnKill();
    }

    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        OnKill();
    }

    void OnKill()
    {
        if (Random.value > healChances[count - 1])
            return;

        Player.Instance.AddHp(healAmounts[count - 1]);
    }

    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"처치 시 {healChances[lv - 1] * 100:0}% 확률로 체력 회복 +{healAmounts[lv - 1]}";
    }
}

using UnityEngine;

// 처치 시 5% 확률로 체력 회복 +2
public class DrainItem : Item
{
    float[] healChances = { 0.2f, 0.3f, 0.4f };
    int[] healAmounts = { 3, 4, 5 };

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

        Character.Instance.AddHp(healAmounts[count - 1]);
    }

    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"처치 시 {healChances[lv - 1] * 100:0}% 확률로 체력 회복 +{healAmounts[lv - 1]}";
    }
}

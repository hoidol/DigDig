using UnityEngine;

// 처치 시 5% 확률로 체력 회복 +2
public class DrainItem : Item
{
    float healChance =  0.05f;
    int healAmount = 3;

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
        if (Random.value > healChance + (0.01 *(count-1)))
            return;

        Character.Instance.AddHp(healAmount * count);
    }

    public override string GetDescription()
    {
        return $"처치 시 {healChance * 100:0}% 확률로 체력 회복 +{healAmount}";
        //return string.Format(TranslateManager.GetText($"{key}_Desc"),healChance * 100,healAmount);
    }
}

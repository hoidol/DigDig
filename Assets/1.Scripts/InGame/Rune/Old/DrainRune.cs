using UnityEngine;

// 처치 시 5% 확률로 체력 회복 +2
public class DrainRune : Rune
{
    float healChance =  0.05f;
    int healAmount = 3;

    void OnEnable()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    void OnDisable()
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        OnKill();
    }

    void OnKill()
    {
        if (Random.value > healChance)
            return;

        Slime.Instance.AddHp(healAmount);
    }

    public override string GetDescription()
    {
        return $"처치 시 {healChance * 100:0}% 확률로 체력 회복 +{healAmount}";
        //return string.Format(TranslateManager.GetText($"{key}_Desc"),healChance * 100,healAmount);
    }
}

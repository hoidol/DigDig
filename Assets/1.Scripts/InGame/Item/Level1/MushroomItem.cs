using UnityEngine;

// 회복	- 적 처치 당 2% 확률로 체력 1 회복
// 자연	행운
public class MushroomItem : Item
{
    float recoverChance = 2f;
    float recoverHp = 1f;

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
        if (Random.Range(0f, 100f) >= recoverChance)
            return;

        Character.Instance.AddHp(recoverHp);
    }

    public override string GetDescription()
    {
        return $"적 처치 당 {recoverChance}% 확률로 체력 +{recoverHp} 회복";
        //return string.Format(TranslateManager.GetText($"{key}_Desc"),recoverChance,recoverHp);
    }
}

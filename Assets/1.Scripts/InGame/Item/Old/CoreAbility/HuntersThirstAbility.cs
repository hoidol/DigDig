using UnityEngine;

// 사냥꾼의 갈증 - 적 처치 시 흡혈
public class HuntersThirstAbility : SynergyAbility
{
    public override string GetDescription(bool detail = false)
    {
        return "적 처치 시 5% 확률로 최대 체력의 1% 추가 회복";
    }
    public override void OnEquip(Character player)
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
    }

    public override void OnUnequip(Character player)
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
    }

    void OnEnemyDead(EnemyDeadEvent e)
    {
        if (Random.Range(0f, 100f) <= 5)
        {
            Debug.Log("HuntersThirstAbility 체력 회복");
            float healAmount = Character.Instance.statMgr.MaxHp * 0.01f;
            Character.Instance.AddHp(healAmount);
        }

    }
}

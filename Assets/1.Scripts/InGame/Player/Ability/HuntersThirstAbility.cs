using UnityEngine;

// 사냥꾼의 갈증 - 적 처치 시 흡혈
public class HuntersThirstAbility : Ability
{
    public override string GetDescription(int c = -1)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;

        return $"적 처치 시 5% 확률로 최대 체력에 {(0.01f * c) * 100}% 추가 회복";
    }
    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
    }

    void OnEnemyDead(EnemyDeadEvent e)
    {
        if (Random.Range(0f, 100f) <= 5)
        {
            float healAmount = Player.Instance.statMgr.MaxHp * 0.01f * count;
            Player.Instance.AddHp(healAmount);
        }

    }
}

using UnityEngine;

// 집중력 유지 - 적 처치 시 다음 공격 시 1번 추가탄 발사
public class FocusSustainAbility : Ability
{
    public override string GetDescription(bool detail = false)
    {
        return "적 처치 시 다음 공격 시 1번 추가탄 발사";
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
        // Player.Instance.QueueExtraShot(1);
    }
}

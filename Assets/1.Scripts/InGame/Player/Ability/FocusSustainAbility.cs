using UnityEngine;

// 집중력 유지 - 적 처치 시 count만큼 추가 발사
public class FocusSustainAbility : Ability
{
    public override string GetDescription(int c = -1)
    {
        if (c == -1)
            c = count;
        if (count <= 0)
            c = 1;

        return $"적 처치 시 다음 공격 시 {c}번 추가탄 발사";
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
        Player.Instance.QueueExtraShot(count);
    }
}

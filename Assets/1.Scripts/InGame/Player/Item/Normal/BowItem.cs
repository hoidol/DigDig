using UnityEngine;

// 적 처치 시 다음 공격 1회 좌우 ±40도 확산탄 발사
public class BowItem : Item, IPreAttack
{
    bool extraShot;

    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
        extraShot = false;
    }

    void OnEnemyDead(EnemyDeadEvent e)
    {
        extraShot = true;
    }

    public void OnPreAttack(Player player, Vector2 dir)
    {
        if (!extraShot) return;
        extraShot = false;
        player.weapon.RequestSpread(2);
    }
}

using UnityEngine;
using Cysharp.Threading.Tasks;

// 적 처치 시 다음 공격 1회 추가 발사
public class BowItem : Item, IComboAttackItem
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

    public async UniTask OnAttack(Player player, Vector2 dir)
    {
        if (!extraShot) return;
        extraShot = false;

        for (int i = 0; i < count; i++)
        {
            await UniTask.Delay(Player.COMBO_ATTACK_INTERVAL_MS);
            Player.Instance.Attack(dir, false);
        }
    }
}

using UnityEngine;
using Cysharp.Threading.Tasks;

//광석 부수면 다음 공격한발더
public class MinerReflexItem : Item//, IComboAttack
{
    bool extraShot;

    public override void OnEquip()
    {
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStone);
    }

    public override void OnUnequip()
    {
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStone);
        extraShot = false;
    }

    void OnDestroyedStone(DestroyedStoneEvent e)
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
            // Player.Instance.Attack(dir, false);
            Player.Instance.weapon.Shoot(new NormalBullet(), dir);
        }
    }
}

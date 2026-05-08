using UnityEngine;
using Cysharp.Threading.Tasks;

//광석 부수면 다음 공격한발더
public class MinerReflexItem : Item, IComboAttackItem
{
    bool extraShot;

    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<OreStoneDestroyedEvent>(OnOreDestroyed);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<OreStoneDestroyedEvent>(OnOreDestroyed);
        extraShot = false;
    }

    void OnOreDestroyed(OreStoneDestroyedEvent e)
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

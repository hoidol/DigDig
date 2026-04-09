using UnityEngine;

// 멀티샷 - 장전 완료 후 첫 발은 멀티샷
public class MultiShotOnReloadAbility : Ability, IPreAttack
{
    bool firstShot;
    public override string GetDescription(int c = -1)
    {
        return $"장전 후 첫발 멀티샷";
    }

    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<ReloadEndEvent>(OnReloadEnd);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<ReloadEndEvent>(OnReloadEnd);
        firstShot = false;
    }

    void OnReloadEnd(ReloadEndEvent e) => firstShot = true;

    public void OnPreAttack(Player player, Vector2 dir)
    {
        if (!firstShot) return;
        firstShot = false;
        player.RequestSpread(count + 1); // count=1→spread2→총3발, count=2→spread3→총4발, count=3→spread4→총5발
    }
}

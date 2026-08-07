using UnityEngine;

// 과적재 - 장전 완료 후 첫 발은 멀티샷
public class MultiShotOnReloadAbility : Ability//, IPreAttack
{
    bool firstShot;

    public override string GetDescription(bool detail = false)
    {
        return "장전 후 첫발 멀티샷";
    }

    public override void OnEquip(Character player)
    {
        GameEventBus.Subscribe<ReloadEndEvent>(OnReloadEnd);
    }

    public override void OnUnequip(Character player)
    {
        GameEventBus.Unsubscribe<ReloadEndEvent>(OnReloadEnd);
        firstShot = false;
    }

    void OnReloadEnd(ReloadEndEvent e) => firstShot = true;

    public void OnPreAttack(ref Bullet bullet, Vector2 dir)
    {
        if (!firstShot) return;
        firstShot = false;
        // player.weapon.RequestMulti(1);
    }
}

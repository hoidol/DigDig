using UnityEngine;

public class PredationAbility : SynergyAbility, IPreFire
{
    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnPreFire(ref Bullet bullet, Vector2 dir)
    {
        // player.weapon.RequestSpread(2);
    }
}

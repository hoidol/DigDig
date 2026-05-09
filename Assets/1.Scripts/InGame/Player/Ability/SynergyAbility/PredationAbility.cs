using UnityEngine;

public class PredationAbility : SynergyAbility, IPreAttack
{
    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnPreAttack(Player player, Vector2 dir)
    {
        player.weapon.RequestSpread(2);
    }
}

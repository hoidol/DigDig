using UnityEngine;

public class CrystalExplosionItem : Item
{
    public float explosionRadius = 3f;
    public float explosionDamage = 20f;
    public LayerMask enemyLayer;

    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<OreStoneDestroyedEvent>(OnOreDestroyed);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<OreStoneDestroyedEvent>(OnOreDestroyed);
    }

    void OnOreDestroyed(OreStoneDestroyedEvent e)
    {
        AOEUtil.DamageEnemies(
            e.oreStone.transform.position,
            explosionRadius * count,
            explosionDamage * count,
            enemyLayer
        );
    }
}

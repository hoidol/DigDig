using UnityEngine;

public class CrystalExplosionItem : Item
{
    public float explosionRadius = 3f;
    public float explosionDamage = 20f;
    public LayerMask enemyLayer;

    public override void OnEquip(Player player)
    {
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStone);
    }

    public override void OnUnequip(Player player)
    {
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStone);
    }

    void OnDestroyedStone(DestroyedStoneEvent e)
    {
        InGameUtil.DamageEnemies(
            e.stone.transform.position,
            explosionRadius * GetLevel(),
            explosionDamage * GetLevel(),
            enemyLayer
        );
    }
}

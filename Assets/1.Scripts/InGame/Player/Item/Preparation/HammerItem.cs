using UnityEngine;

//안씀
public class HammerItem : Item, IBulletItem
{
    public float explosionChance = 0.3f;
    public float explosionRadius = 2.5f;
    public float explosionDamage = 15f;
    public LayerMask enemyLayer;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBehavior(new HammerExplosionBehavior(
            explosionChance,
            explosionRadius * count,
            explosionDamage * count,
            enemyLayer
        ));
    }
}

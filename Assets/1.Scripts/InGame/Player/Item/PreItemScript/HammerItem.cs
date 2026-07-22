using UnityEngine;

//안씀
public class HammerItem : Item, IBullet
{
    public float explosionChance = 0.3f;
    public float explosionRadius = 2.5f;
    public float explosionDamage = 15f;
    public LayerMask enemyLayer;


    public void OnBulletFired(PlayerBulletObject bullet)
    {
        bullet.AddBehavior(new HammerExplosionBehavior(
            explosionChance,
            explosionRadius * count,
            explosionDamage * count,
            enemyLayer
        ));
    }
}

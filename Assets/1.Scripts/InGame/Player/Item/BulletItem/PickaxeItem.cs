public class PickaxeItem : Item, IBulletItem
{
    public float bonusDamageRate = 0.1f;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBulletForce(new OreDamageBehavior(bonusDamageRate));
    }
}

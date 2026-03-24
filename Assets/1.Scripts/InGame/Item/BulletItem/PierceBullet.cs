public class PierceBullet : Item, IBulletItem
{
    public int pierceCount = 2;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBehavior(new PierceBehavior(pierceCount));
    }
}

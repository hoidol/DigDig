public class PierceBullet : Item, IBulletItem
{
    public int basePierceCount = 0;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBehavior(new PierceBehavior(count + basePierceCount));
    }
}

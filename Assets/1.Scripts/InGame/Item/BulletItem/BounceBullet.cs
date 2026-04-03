public class BounceBullet : Item, IBulletItem
{
    public int baseBounceCount = 0;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBehavior(new BounceBehavior(count + baseBounceCount));
    }
}

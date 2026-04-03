public class TargetBlueItem : Item, IBulletItem
{
    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBehavior(new PierceBehavior(count));
    }
}

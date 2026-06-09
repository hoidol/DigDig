public class TargetBlueItem : Item, IBullet
{
    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBulletObject bullet)
    {
        bullet.AddBehavior(new PierceBehavior(GetLevel()));
    }
}

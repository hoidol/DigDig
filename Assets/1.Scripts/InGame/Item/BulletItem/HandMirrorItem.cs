using UnityEngine;
public class HandMirrorItem : Item, IBulletItem
{
    public int bounceCount = 1;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        Debug.Log("HandMirrorItem OnBulletFired");
        bullet.AddBehavior(new BounceBehavior(count));
    }
}

using UnityEngine;

public class SnowflakeItem : Item, IBulletItem
{
    public float[] freezeChances = { 0.05f, 0.1f, 0.2f }; // 5%
    public float[] freezeDurations = { 2f, 2.5f, 3 };

    public override void OnUnequip(Player player) { }
    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBehavior(new FreezeOnHitBehavior(freezeChances[count - 1], freezeDurations[count - 1]));
    }
}

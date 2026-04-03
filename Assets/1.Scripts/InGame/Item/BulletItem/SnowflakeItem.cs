using UnityEngine;

public class SnowflakeItem : Item, IBulletItem
{
    public float freezeChance = 0.05f; // 5%
    public float freezeDuration = 2f;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        Debug.Log("SnowflakItem OnBulletFired");
        bullet.AddBehavior(new FreezeOnHitBehavior(freezeChance, freezeDuration));
    }
}

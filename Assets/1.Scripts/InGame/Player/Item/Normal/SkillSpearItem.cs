using UnityEngine;

public class SkillSpearItem : Item, IBulletItem
{
    public float instakillChance = 0.01f; // 1%

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBehavior(new InstakillBehavior(instakillChance * count));
    }
}

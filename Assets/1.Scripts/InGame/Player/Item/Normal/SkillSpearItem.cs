using UnityEngine;

// 스킬 창: 1% 확률로 즉사
public class SkillSpearItem : Item, IBulletItem
{
    const float INSTAKILL_CHANCE = 0.01f;

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBehavior(new InstakillBehavior(INSTAKILL_CHANCE));
    }
}

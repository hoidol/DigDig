using UnityEngine;

// 눈꽃: 30% 확률로 빙결탄 발사
public class SnowflakeItem : Item, IBulletItem
{
    const float CHANCE = 0.30f;
    const float DURATION = 2f;

    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBehavior(new FreezeOnHitBehavior(CHANCE, DURATION));
    }
}

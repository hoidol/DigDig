using UnityEngine;

// 눈꽃: 30% 확률로 빙결탄 발사
public class SnowflakeItem : Item, IBullet
{
    const float CHANCE = 0.30f;
    const float DURATION = 2f;


    public void OnBulletFired(PlayerBulletObject bullet)
    {
        bullet.AddBehavior(new FreezeOnHitBehavior(CHANCE, DURATION));
    }
}

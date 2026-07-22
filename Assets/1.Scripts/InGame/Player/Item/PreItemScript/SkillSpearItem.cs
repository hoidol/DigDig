using UnityEngine;

// 스킬 창: 1% 확률로 즉사
public class SkillSpearItem : Item, IBullet
{
    const float INSTAKILL_CHANCE = 0.01f;


    public void OnBulletFired(PlayerBulletObject bullet)
    {
        bullet.AddBehavior(new InstakillBehavior(INSTAKILL_CHANCE));
    }
}

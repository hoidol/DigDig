using UnityEngine;

public class FlameBulletItem : Item, IBullet
{
    static readonly float duration = 3f;
    static readonly float dpsValue = 2f;
    static readonly float chance = 20f;


    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"{chance}% 확률로 불꽃탄 발사";
    }



    public void OnBulletFired(PlayerBulletObject bullet)
    {
        if (Random.Range(0f, 100f) < chance)
            bullet.AddBehavior(new FlameOnHitBehavior(duration, dpsValue));
    }
}

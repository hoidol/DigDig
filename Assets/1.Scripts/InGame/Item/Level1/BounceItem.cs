using UnityEngine;

public class BounceItem : Item,IFired
{
    int bounceCount = 2;
    public void OnFired(ref Bullet bullet, ref CharacterBulletObject playerBulletObject, Vector2 dir)
    {
        playerBulletObject.AddBehavior(new BounceBehavior(count * bounceCount));
    }


    public override string GetDescription()
    {
        return $"탄 튕김 +{bounceCount}";
    }
}
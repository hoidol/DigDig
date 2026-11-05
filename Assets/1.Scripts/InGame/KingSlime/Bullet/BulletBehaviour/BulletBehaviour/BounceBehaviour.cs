using UnityEngine;
public class BounceBehavior : IBulletBehavior
{
    public int ApplyOrder => 0;
    int remaining;
    float multi;
    public BounceBehavior(int count, float mul)
    {
        remaining = count;
        multi = mul;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        bullet.damageMultiplier *= multi;
        if (remaining-- <= 0)
            return true;

        bullet.Bounce(hit2D);
        return false;
    }
}

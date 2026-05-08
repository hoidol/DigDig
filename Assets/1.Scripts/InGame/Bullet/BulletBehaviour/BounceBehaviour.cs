using UnityEngine;
public class BounceBehavior : IBulletBehavior
{
    int remaining;
    public BounceBehavior(int count) { remaining = count; }

    public bool OnHit(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        bullet.damageMultiplier *= 0.75f;
        if (remaining-- <= 0 || bullet.damage * bullet.damageMultiplier < 1f)
            return true;
        bullet.Bounce(hit2D);
        return false;
    }
    public void OnMove(BulletBase bullet) { }
    public void Merge(IBulletBehavior other)
    {
        remaining += ((BounceBehavior)other).remaining;
    }
}

using UnityEngine;
public class BounceBehavior : IBulletBehavior
{
    int remaining;
    public BounceBehavior(int count) { remaining = count; }

    public bool OnHit(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        if (remaining-- > 0)
        {
            bullet.Bounce(hit2D);
            return false;
        }
        return true;
    }
    public void OnMove(BulletBase bullet) { }
    public void Merge(IBulletBehavior other)
    {
        remaining += ((BounceBehavior)other).remaining;
    }
}

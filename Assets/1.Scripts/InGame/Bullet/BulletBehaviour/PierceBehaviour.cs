using UnityEngine;
public class PierceBehavior : IBulletBehavior
{
    int remaining;
    public PierceBehavior(int count) { remaining = count; }

    public bool OnHit(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        return --remaining <= 0;
    }
    public void OnMove(BulletBase bullet) { }
    public void Merge(IBulletBehavior other)
    {
        remaining += ((PierceBehavior)other).remaining;
    }
}

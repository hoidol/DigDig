using UnityEngine;
public class PierceBehavior : IBulletBehavior
{
    int remaining;
    public PierceBehavior(int count) { remaining = count; }

    public bool OnHit(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        bullet.damageMultiplier *= 0.75f;
        if (--remaining <= 0 || bullet.damage * bullet.damageMultiplier < 1f)
            return true;
        return false;
    }
    public void OnMove(BulletBase bullet) { }
    public void Merge(IBulletBehavior other)
    {
        remaining += ((PierceBehavior)other).remaining;
    }
}

using UnityEngine;
public class PierceBehavior : IBulletBehavior
{
    int remaining;
    public PierceBehavior(int count) { remaining = count; }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D)
    {
        bullet.damageMultiplier *= Player.Instance.statMgr.AmmoEfficiency;
        if (--remaining <= 0)
            return true;
        return false;
    }
    public void OnMove(BulletObject bullet) { }
    public void Merge(IBulletBehavior other)
    {
        remaining += ((PierceBehavior)other).remaining;
    }
}

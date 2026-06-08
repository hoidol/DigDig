using UnityEngine;
public class BounceBehavior : IBulletBehavior
{
    int remaining;
    public BounceBehavior(int count) { remaining = count; }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D)
    {

        bullet.damageMultiplier *= Player.Instance.statMgr.AmmoEfficiency;
        if (remaining-- <= 0)
            return true;

        bullet.Bounce(hit2D);
        return false;
    }
    public void OnMove(BulletObject bullet) { }
    public void Merge(IBulletBehavior other)
    {
        remaining += ((BounceBehavior)other).remaining;
    }
}

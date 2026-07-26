using UnityEngine;
public class BounceBehavior : IBulletBehavior
{
    int remaining;
    public BounceBehavior(int count) { remaining = count; }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        Debug.Log($"BounceBehavior OnHit {remaining}");
        bullet.damageMultiplier *= Player.Instance.statMgr.AmmoEfficiency;
        if (remaining-- <= 0)
            return true;


        bullet.Bounce(hit2D);
        return false;
    }
}

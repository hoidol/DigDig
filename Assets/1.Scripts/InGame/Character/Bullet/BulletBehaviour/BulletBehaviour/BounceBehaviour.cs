using UnityEngine;
public class BounceBehavior : IBulletBehavior
{
    int remaining;
    public BounceBehavior(int count) { remaining = count; }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        // bullet.damageMultiplier *= Character.Instance.statMgr.AmmoEfficiency; //1에 가까울수록 효율 좋음
        if (remaining-- <= 0)
            return true;


        bullet.Bounce(hit2D);
        return false;
    }
}

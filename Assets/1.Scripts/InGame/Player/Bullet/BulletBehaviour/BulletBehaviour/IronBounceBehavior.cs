// 철탄 전용 바운스 - 데미지 감소 없이 튕김
using UnityEngine;
public class IronBounceBehavior : IBulletBehavior
{
    int remaining; 
    int ignoreCount; 
    public IronBounceBehavior(int bounceCount, int iCount) 
    { 
        remaining = bounceCount; 
        ignoreCount = iCount;
    }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {

        if (ignoreCount<0)
        {
            bullet.damageMultiplier *= Player.Instance.statMgr.AmmoEfficiency;
        }

        if (remaining-- <= 0) return true;
        ignoreCount--;
        bullet.Bounce(hit2D);
        return false;
    }

}

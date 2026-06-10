// 철탄 전용 바운스 - 데미지 감소 없이 튕김
using UnityEngine;
public class IronBounceBehavior : IBulletBehavior
{
    int remaining;
    public IronBounceBehavior(int count) { remaining = count; }

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (remaining-- <= 0) return true;
        bullet.Bounce(hit2D);
        return false;
    }

    public void OnMove(BulletObject bullet) { }

    public void Merge(IBulletBehavior other)
    {
        remaining += ((IronBounceBehavior)other).remaining;
    }
}

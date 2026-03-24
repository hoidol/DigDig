public class BounceBehavior : IBulletBehavior
{
    int remaining;
    public BounceBehavior(int count) { remaining = count; }

    public bool OnHit(BulletBase bullet, IHittable hit)
    {
        if (remaining-- > 0)
        {
            bullet.Bounce();
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

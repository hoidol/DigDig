public interface IBulletBehavior
{
    bool OnHit(BulletBase bullet, IHittable hit);
    void OnMove(BulletBase bullet);
    void Merge(IBulletBehavior other);
}

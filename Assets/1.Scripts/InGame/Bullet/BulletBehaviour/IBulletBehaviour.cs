using UnityEngine;
public interface IBulletBehavior
{
    bool OnHit(BulletBase bullet, IHittable hit, RaycastHit2D hit2D);
    void OnMove(BulletBase bullet);
    void Merge(IBulletBehavior other); // 능력치 증가
}

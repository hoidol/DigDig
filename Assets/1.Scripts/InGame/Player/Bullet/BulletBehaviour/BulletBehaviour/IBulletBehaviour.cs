using UnityEngine;
public interface IBulletBehavior
{
    bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D);
    void OnMove(BulletObject bullet);
    void Merge(IBulletBehavior other); // 능력치 증가
}

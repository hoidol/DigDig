using UnityEngine;
public interface IBulletBehavior
{
    bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir);
    //void OnMove(BulletObject bullet);
    // void Merge(IBulletBehavior other); // 능력치 증가
}

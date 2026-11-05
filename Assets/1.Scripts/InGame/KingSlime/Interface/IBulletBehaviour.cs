using UnityEngine;
public interface IBulletBehavior
{
    bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir);
    int ApplyOrder //적용 우선 순위 설정
    {
        get;
    }
    //void OnMove(BulletObject bullet);
    // void Merge(IBulletBehavior other); // 능력치 증가
}

using UnityEngine;
public interface IGun : IWeapon
{

    PlayerBullet Shoot(Vector2 dir, Vector2 pos);
    void QueueExtraShot(int count = 1);
    void ResetOnUndergroundStart();
    int CurBulletCount { get; }
    bool IsReloading { get; }
    void AddBullet(int count = 1);
}
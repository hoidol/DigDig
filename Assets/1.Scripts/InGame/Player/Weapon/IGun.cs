using UnityEngine;
public interface IGun : IWeapon
{

    PlayerBullet Shoot(Vector2 dir, Vector2 pos);
    void QueueExtraShot(int count = 1);
    void RequestMulti(int count);
    // 다음 Attack에서 발사할 확산탄 수 누적
    void RequestSpread(int count);
    //void RequestRadialShot(int count);
    void ResetOnUndergroundStart();
    int CurBulletCount { get; }
    bool IsReloading { get; }
    void AddBullet(int count = 1);
}
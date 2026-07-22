using UnityEngine;
public interface IGun : IWeapon
{

    PlayerBulletObject Shoot(Bullet bullet, Vector2 dir, Vector2 pos);
    // void QueueExtraShot(int count = 1);
    // void RequestMulti(int count);
    // 다음 Attack에서 발사할 확산탄 수 누적
    // void RequestSpread(int count);
    //void RequestRadialShot(int count);
    // int CurBulletOrder { get; }
    // bool IsReloading { get; }
}
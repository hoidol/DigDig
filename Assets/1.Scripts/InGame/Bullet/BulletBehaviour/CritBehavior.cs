// 강제 크리티컬 - 명중 시 CritPower 배율 적용
using UnityEngine;

public class CritBehavior : IBulletForce
{

    public float GetMultiDamage(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        PlayerBullet pb = bullet as PlayerBullet;
        pb.damageData.mustCrit = true;
        return 0;
    }
}

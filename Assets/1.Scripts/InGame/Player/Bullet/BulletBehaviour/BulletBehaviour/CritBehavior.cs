// 강제 크리티컬 - 명중 시 CritPower 배율 적용
using UnityEngine;

public class CritBehavior : IBulletForce
{

    public float GetMultiDamage(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        PlayerBulletObject pb = bullet as PlayerBulletObject;
        pb.damageData.mustCrit = true;
        return 0;
    }
}

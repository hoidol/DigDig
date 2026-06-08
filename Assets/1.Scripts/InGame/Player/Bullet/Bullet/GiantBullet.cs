using UnityEngine;

// [거대탄]
// 탄 크기 2배, 데미지 1.5× (multiplyATK=1.5 → boost=+0.5×)
public class GiantBullet : Bullet
{
    const float SCALE = 2.5f;

    public override void OnBulletFired(PlayerBulletObject bullet)
    {
        base.OnBulletFired(bullet); // scale → 1 리셋 후 다시 설정
        bullet.transform.localScale = Vector3.one * SCALE;
        bullet.AddBulletForce(new DamageBoostForce(bulletData.multiplyATK - 1f)); // +0.5×
    }

    public override string GetDescription(bool detail = false) => $"탄 크기 {SCALE}배, 데미지 1.5×";
    public override PlayerBulletObject GetBulletObject()
    {
        return null;
    }
}

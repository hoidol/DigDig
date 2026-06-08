using UnityEngine;

// [산탄]
// 발사 시 2발 추가 확산 (총 3방향), 탄당 데미지 0.5×
public class ScatterBullet : Bullet, IPreAttack
{
    const int EXTRA_SPREAD = 2; // 좌우 각 1발씩 추가

    public void OnPreAttack(Player player, Vector2 dir, int shotOrder)
    {
        // player.weapon.RequestSpread(EXTRA_SPREAD);
    }

    public override void OnBulletFired(PlayerBulletObject bullet)
    {
        bullet.transform.localScale = Vector3.one;
        bullet.AddBehavior(new BounceBehavior(Player.Instance.bounce));
        // 0.5× 데미지: base(1×) + boost(-0.5×) = 0.5×
        bullet.AddBulletForce(new DamageBoostForce(bulletData.multiplyATK - 1f)); // -0.5
    }

    public override string GetDescription(bool detail = false)
        => "3방향 발사, 탄당 데미지 0.5×";

    public override PlayerBulletObject GetBulletObject()
    {
        return null;
    }
}

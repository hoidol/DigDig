using UnityEngine;

// [산탄]
// 발사 시 2발 추가 확산 (총 3방향), 탄당 데미지 0.5×
public class ScatterBulletSpec : BulletSpec
{
    const int EXTRA_SPREAD = 2; // 좌우 각 1발씩 추가
    public float[] multiplyATKs = { 0.5f, 0.6f, 0.7f };
    public ScatterBulletSpec()
    {
        key = "Scatter";
    }
    public void OnPreAttack(ref BulletSpec bullet, Vector2 dir)
    {
        // player.weapon.RequestSpread(EXTRA_SPREAD);
    }

    // public override void OnBulletFired(PlayerBulletObject bullet)
    // {
    //     bullet.transform.localScale = Vector3.one;
    //     bullet.AddBehavior(new BounceBehavior(Player.Instance.bounce));
    //     // 0.5× 데미지: base(1×) + boost(-0.5×) = 0.5×
    //     bullet.AddBulletForce(new DamageBoostForce(multiplyATKs[GetLevel() - 1])); // -0.5
    // }

    // public override string GetDescription(int lv = 1, bool detail = false)
    //     => $"3방향 발사, 기본 데미지 피해 {(multiplyATKs[lv - 1] - 1) * 100}%";


}

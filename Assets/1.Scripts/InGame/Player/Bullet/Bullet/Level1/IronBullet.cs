// [철탄]
// 튕겨도 데미지 감소 없음 (기본 BounceBehavior의 0.6× 감소 제거)
public class IronBullet : Bullet
{
    public IronBullet()
    {
        key = "Iron";
    }
    public override void OnBulletFired(PlayerBulletObject bullet)
    {
        bullet.transform.localScale = UnityEngine.Vector3.one;
        bullet.AddBehavior(new IronBounceBehavior(Player.Instance.bounce));
    }

    public override string GetDescription(bool detail = false) => "튕겨도 데미지 감소 없음";

}

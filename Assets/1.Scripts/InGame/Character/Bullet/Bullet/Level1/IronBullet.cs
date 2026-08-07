// [철탄]
// 튕겨도 데미지 감소 없음 (기본 BounceBehavior의 0.6× 감소 제거)
public class IronBullet : Bullet
{
    public IronBullet()
    {
        key = "Iron";
    }
    int[] ignoreBouncEfficiency = {4,6,7};
    // public override void OnBulletFired(PlayerBulletObject bullet)
    // {
    //     bullet.transform.localScale = UnityEngine.Vector3.one;
    //     bullet.AddBehavior(new IronBounceBehavior(Player.Instance.bounce, ignoreBouncEfficiency[GetLevel()-1]));
    // }

    // public override string GetDescription(int lv = 1, bool detail = false) => "튕겨도 데미지 감소 없음";

}

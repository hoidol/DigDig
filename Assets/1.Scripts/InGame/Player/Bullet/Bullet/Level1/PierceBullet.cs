// [숫돌]
// 발사된 총알에 관통(PierceBehavior)을 추가하는 IBullet.
// count + basePierceCount만큼 관통 횟수가 설정되어, 총알이 여러 적/광석을 연속으로 통과.
public class PierceBullet : Bullet
{
    public int pierceCount = 2;
    public PierceBullet()
    {
        key = "Pierce";
    }
    public override void OnBulletFired(PlayerBulletObject bullet)
    {
        base.OnBulletFired(bullet);
        bullet.AddBehavior(new PierceBehavior(pierceCount));
    }

    public override string GetDescription(bool detail = false)
    {
        return $"탄 모두 튕긴 후 관통 +{pierceCount}";
    }


}

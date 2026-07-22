// [분열탄]
// 처치 시 3방향으로 분열탄 발사 (데미지 절반, 튕김 없음)
public class SplitBullet : Bullet
{
    const int SPLIT_COUNT = 3;
    const float DAMAGE_RATIO = 0.5f;

    public float[] multiplyATKs = {0.5f,0.6f,0.7f};
    public SplitBullet()
    {
        key = "Split";
    }
    // public override void OnBulletFired(PlayerBulletObject bullet)
    // {
    //     base.OnBulletFired(bullet);
    //     bullet.AddBehavior(new SplitOnKillBehavior(SPLIT_COUNT, DAMAGE_RATIO));
    // }

    // public override string GetDescription(int lv = 1, bool detail = false)
    //     => $"처치 시 {SPLIT_COUNT}방향 분열탄 (데미지 {DAMAGE_RATIO * 100:0}%)";

}

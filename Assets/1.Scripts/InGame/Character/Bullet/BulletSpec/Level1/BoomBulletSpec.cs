// [숫돌]
// 발사된 총알에 관통(PierceBehavior)을 추가하는 IBullet.
// count + basePierceCount만큼 관통 횟수가 설정되어, 총알이 여러 적/광석을 연속으로 통과.
public class BoomBulletSpec : BulletSpec
{
    // public int[] pierceCounts = {2,3,4};
    // public float[] multiplyATKs = {1.3f,1.4f,1.5f};
    public float boomRange = 2.5f;
    public float damage;
    // public float multiplyAtk = 1;

    public BoomBulletSpec()
    {
        key = "Boom";
    }
    // public override void OnBulletFired(PlayerBulletObject bullet)
    // {
    //     base.OnBulletFired(bullet);
    //     bullet.AddBulletForce(new DamageBoostForce(multiplyATKs[GetLevel()-1])); // +0.5×
    //     bullet.AddBehavior(new PierceBehavior(pierceCounts[GetLevel()-1]));
    // }

    // public override string GetDescription(int lv, bool detail = false)
    // {
    //     return $"튕긴 후 관통 +{pierceCounts[lv-1]} 피해 {(multiplyATKs[lv-1]-1)*100}% 증가";
    // }


}

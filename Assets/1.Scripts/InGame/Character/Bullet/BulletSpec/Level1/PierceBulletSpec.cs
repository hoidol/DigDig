// [숫돌]
// 발사된 총알에 관통(PierceBehavior)을 추가하는 IBullet.
// count + basePierceCount만큼 관통 횟수가 설정되어, 총알이 여러 적/광석을 연속으로 통과.
public class PierceBulletSpec : AllyBulletSpec
{
    public int pierceCount;
    public PierceBulletSpec()
    {
        key = "Pierce";
    }
    public override void StartBulletSpec()
    {
        base.StartBulletSpec();
        AddBulletBehaviour(new PierceBehavior(pierceCount));
    }



}

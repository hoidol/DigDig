using UnityEngine;

// [불탄]
// 적중 시 30% 확률로 화상 적용
public class BounceBulletSpec : AllyBulletSpec
{
    public int bounce;

    public BounceBulletSpec()
    {
        key = "Bounce";
    }
    
    public override void StartBulletSpec()
    {
        base.StartBulletSpec();
        AddBulletBehaviour(new BounceBehavior(bounce));
    }

}

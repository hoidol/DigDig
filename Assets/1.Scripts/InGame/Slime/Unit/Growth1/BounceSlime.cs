using UnityEngine;

public class BounceSlime : SlimeGrowth1
{
    BounceBulletSpec bounceBullet;
    int[] bounces = {1, 2, 3};
    public override void Awake()
    {
        base.Awake();
        attackPowers = new float[] {6,20,30};
        attackSpeeds = new float[] {10,20,30};
        bounceBullet = new BounceBulletSpec();
        bounceBullet.bounce = bounces[level];
    }

    public override AllyBulletObject GetBullet()
    {
        return bounceBullet.Instantiate(this);
    }


    public override string GetDescription(int level =0)
    {
        return "튕기는 탄을 발사합니다";
    }
}
using UnityEngine;

public class BounceSlime : SlimeGrowth1
{
    BounceBulletSpec bounceBullet;
    int[] bounces = {1, 2, 3};
    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);

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
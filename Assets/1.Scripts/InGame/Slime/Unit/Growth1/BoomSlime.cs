using UnityEngine;

public class BoomSlime : SlimeGrowth1
{
    BoomBulletSpec boomBulletSpec;
    float[] boomRanges = {2f,2.5f,3f};
    public override void Awake()
    {
        base.Awake();
        // UserSlime userSlime = UserManager.Instance.userSlimeManager.GetUserSlime(key);
        // userSlime.EnhanceLevel();
    }

    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);

        boomBulletSpec = new BoomBulletSpec();
        boomBulletSpec.boomRange = boomRanges[level];
        boomBulletSpec.damage = AttackPower;
    }

    public override AllyBulletObject GetBullet()
    {
        return boomBulletSpec.Instantiate(this);
    }

    public override string GetDescription(int level =0)
    {
        return "폭탄을 발사합니다.";
    }
}
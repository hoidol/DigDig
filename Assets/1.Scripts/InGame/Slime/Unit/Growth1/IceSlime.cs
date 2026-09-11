using UnityEngine;

public class IceSlime : SlimeGrowth1
{
    IceBulletSpec iceBulletSpec;
    float[] durations = {3f,4.5f,6f};
    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);

        iceBulletSpec = new IceBulletSpec();
        iceBulletSpec.damage= AttackPower;
        iceBulletSpec.duration = durations[level];
    }
    
    public override AllyBulletObject GetBullet()
    {
        return iceBulletSpec.Instantiate(this);
    }

    public override string GetDescription(int level =0)
    {
        return "얼음을 쏩니다";
    }
}
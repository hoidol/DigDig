using UnityEngine;

public class FlameMiniMe : MiniMeGrowth1
{
    FlameBulletSpec flameBullet;
    float[] burnDurations = {4f, 6f};
    float[] burnDPS = {5f,7f};
    public override void Awake()
    {
        base.Awake();
        
        attackPowers = new float[] {10,20,30};
        attackSpeeds = new float[] {10,20,30};


        flameBullet = new FlameBulletSpec();
        flameBullet.burnDuration = burnDurations[level];
        flameBullet.burnDuration = burnDPS[level];
    }

    public override AllyBulletObject GetBullet()
    {
        return flameBullet.Instantiate(this);
    }


    public override string GetDescription()
    {
        return "화염탄을 발사합니다";
    }
}
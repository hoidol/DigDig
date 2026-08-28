using UnityEngine;

public class PierceMiniMe : MiniMeGrowth1
{
    PierceBulletSpec pierceBullet;
    int[] pierceBullt = {2,3,4};
    public override void Awake()
    {
        base.Awake();
        //public float burnDuration;
        //public float burnDPS;
        attackPowers = new float[] {10,20,30};
        attackSpeeds = new float[] {10,20,30};


        pierceBullet = new PierceBulletSpec();
        pierceBullet.pierceCount = pierceBullt[level];
    }

    public override AllyBulletObject GetBullet()
    {
        return pierceBullet.Instantiate(this);
    }


    public override string GetDescription()
    {
        return "관통탄을 발사합니다";
    }
}
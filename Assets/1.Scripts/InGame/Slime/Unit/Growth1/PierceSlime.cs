using UnityEngine;

public class PierceSlime : SlimeGrowth1
{
    PierceBulletSpec pierceBullet;
    int[] pierceBullt = {2,3,4};

    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);

        pierceBullet = new PierceBulletSpec();
        pierceBullet.pierceCount = pierceBullt[level];
        pierceBullet.damage = AttackPower;
    }
    
    public override AllyBulletObject GetBullet()
    {
        return pierceBullet.Instantiate(this);
    }


    public override string GetDescription(int level =0)
    {
        return "관통탄을 발사합니다";
    }
}
using UnityEngine;

public class FlameSlime : SlimeGrowth1
{
    FlameBulletSpec flameBullet;
    float burnDuration = 4;
    float[] burnDPS = {5f,7f,9f};
    public TriggerItem misfireItem;
    

    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos,lv);


        flameBullet = new FlameBulletSpec();
        flameBullet.burnDuration = burnDuration;
        flameBullet.burnDuration = burnDPS[level];

        if(lv == SlimeData.MAX_LEVEL-1)
        {
            misfireItem.OnEquip();
        }
    }

    public override AllyBulletObject GetBullet()
    {
        return flameBullet.Instantiate(this);
    }


    public override string GetDescription(int level =0)
    {
        return SlimeData.descs[level];
    }
}
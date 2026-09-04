using UnityEngine;

public class FlameSlime : SlimeGrowth1
{
    FlameBulletSpec flameBullet;
    float[] burnDurations = {4f, 5f, 6f};
    float[] burnDPS = {5f,6f,7f};
    public TriggerItem misfireItem;
    //3성때 스킬하나씩 주자
    public override void Awake()
    {
        base.Awake();
        UserSlime userSlime = UserManager.Instance.userSlimeManager.GetUserSlime(key);
        
        userSlime.EnhanceLevel();

        attackPowers = new float[] {10,20,30};
        attackSpeeds = new float[] {10,20,30};


        flameBullet = new FlameBulletSpec();
        flameBullet.burnDuration = burnDurations[level];
        flameBullet.burnDuration = burnDPS[level];
    }

    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos,lv);
        if(lv == SlimeData.MAX_Level-1)
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
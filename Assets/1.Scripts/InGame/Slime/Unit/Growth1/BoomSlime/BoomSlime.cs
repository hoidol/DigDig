using UnityEngine;

public class BoomSlime : SlimeGrowth1
{
    BoomBulletSpec boomBulletSpec;
    // float[] boomRanges = {2f,2.5f,3f};
    // public float multiBoomRange = 1f;
    BoomSlimeData boomSlimeData;
    public float boomRange;
    public int maxHitCount;
    public BoomEnhance8Ability pushEffectAbility;

    public override void Spawn(Vector2 pos, int lv)
    {
        boomSlimeData = SlimeManager.Instance.GetSlimeData(key) as BoomSlimeData;
        base.Spawn(pos, lv);
    }
    public override void InitSlime()
    {
        boomBulletSpec = new BoomBulletSpec();
        boomRange = boomSlimeData.ranges[mergeLevel];
        maxHitCount = boomSlimeData.maxHitCount;
        
        base.InitSlime();
    }


    public override AllyBulletObject GetBullet()
    {
        boomBulletSpec.boomRange = boomRange;
        boomBulletSpec.damage = AttackPower;
        boomBulletSpec.maxHitCount = maxHitCount;
        boomBulletSpec.StartBulletSpec();

        if (pushEffectAbility.isActivate)
        {
            boomBulletSpec.AddBulletBehaviour(new PushBehavior(pushEffectAbility.pushPower));
        }
        return boomBulletSpec.Instantiate(this);
    }

}
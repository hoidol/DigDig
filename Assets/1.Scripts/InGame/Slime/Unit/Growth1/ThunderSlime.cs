using UnityEngine;

public class ThunderSlime : SlimeGrowth1
{
    ThunderBulletSpec thunderBullet;

    float searchRadius = 3f;
    int strikeCount = 1;
    float[] damages = {2f, 3f, 4f}; // 공격력의 100%
    public LayerMask hitLayerMask;
    public override void Awake()
    {
        base.Awake();
        UserSlime userSlime = UserManager.Instance.userSlimeManager.GetUserSlime(key);
        
        userSlime.EnhanceLevel();

        attackPowers = new float[] {10,20,30};
        attackSpeeds = new float[] {10,20,30};


        thunderBullet = new ThunderBulletSpec();
        thunderBullet.searchRadius = searchRadius;
        thunderBullet.strikeCount = strikeCount;
        thunderBullet.damage = damages[level];
        thunderBullet.hitLayerMask = hitLayerMask;
    }

    public override AllyBulletObject GetBullet()
    {
        return thunderBullet.Instantiate(this);
    }


    public override string GetDescription(int level =0)
    {
        return "천둥탄을 발사합니다";
    }
}
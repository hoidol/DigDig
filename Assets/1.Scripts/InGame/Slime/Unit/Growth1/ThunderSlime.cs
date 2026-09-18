using UnityEngine;

public class ThunderSlime : SlimeGrowth1
{
    ThunderBulletSpec thunderBullet;

    float searchRadius = 3f;
    int strikeCount = 1;
    public LayerMask hitLayerMask;


    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);

        
        thunderBullet = new ThunderBulletSpec();
        thunderBullet.searchRadius = searchRadius;
        thunderBullet.strikeCount = strikeCount;
        thunderBullet.damage = AttackPower;
        thunderBullet.hitLayerMask = hitLayerMask;
    }
    

    public override AllyBulletObject GetBullet()
    {
        return thunderBullet.Instantiate(this);
    }


    // public override string GetDescription(int level =0)
    // {
    //     return "천둥탄을 발사합니다";
    // }
}
using UnityEngine;

public class PierceSlime : SlimeGrowth1
{
    public PierceSlimeData pierceSlimeData;
    PierceBulletSpec pierceBulletSpec;
    public int pierceCount;
    public PierceEnhance8Ability damageBoostAbility;
    public PierceEnhance11Ability scaleUpAbility;

    public override void Spawn(Vector2 pos, int lv)
    {
        pierceSlimeData = SlimeManager.Instance.GetSlimeData(key) as PierceSlimeData;
        base.Spawn(pos, lv);
    }
    

    public override void InitSlime()
    {
        pierceBulletSpec = new PierceBulletSpec();
        pierceCount = pierceSlimeData.pierceCount[mergeLevel];

        base.InitSlime();
    }
    public override AllyBulletObject GetBullet()
    {
        pierceBulletSpec.pierceCount = pierceCount;
        pierceBulletSpec.damage = AttackPower;
        pierceBulletSpec.StartBulletSpec();

        AllyBulletObject bulletObj = pierceBulletSpec.Instantiate(this);

        bulletObj.transform.localScale = Vector3.one;
        if (scaleUpAbility.isActivate)
        {
            bulletObj.transform.localScale *= scaleUpAbility.scale;
        }


        if (damageBoostAbility.isActivate)
        {
             pierceBulletSpec.AddBulletForce(new DamageBoostForce(damageBoostAbility.boostForceValue)); 
        }
        return bulletObj;
    }


    // public override string GetDescription(int level =0)
    // {
    //     return "관통탄을 발사합니다";
    // }
}
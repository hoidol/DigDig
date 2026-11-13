using UnityEngine;

// [각성] (진화) 탄 피해 +EVOLVE_DAMAGE_BONUS
public class PierceMinion : MinionGrowth1
{
    public const float EVOLVE_DAMAGE_BONUS = 0.2f;
    public PierceMinionData pierceMinionData;
    PierceBulletSpec pierceBulletSpec;
    public int pierceCount;

    public override void Spawn(Vector2 pos, int lv)
    {
        pierceMinionData = MinionManager.Instance.GetMinionData(key) as PierceMinionData;
        base.Spawn(pos, lv);
    }


    public override void InitMinion()
    {
        pierceBulletSpec = new PierceBulletSpec();
        pierceCount = pierceMinionData.pierceCount.GetByMergeLevel(mergeLevel);


        base.InitMinion();
    }
    public override AllyBulletObject GetBullet()
    {
        pierceBulletSpec.pierceCount = pierceCount;
        pierceBulletSpec.damage = AttackPower;
        pierceBulletSpec.multi = 0.8f;
        pierceBulletSpec.StartBulletSpec();
        if (isEvolved)
            pierceBulletSpec.AddBulletForce(new DamageBoostForce(EVOLVE_DAMAGE_BONUS));

        AllyBulletObject bulletObj = pierceBulletSpec.Instantiate(this);
        bulletObj.transform.localScale = Vector3.one;
        return bulletObj;
    }


    // public override string GetDescription(int level =0)
    // {
    //     return "관통탄을 발사합니다";
    // }
}
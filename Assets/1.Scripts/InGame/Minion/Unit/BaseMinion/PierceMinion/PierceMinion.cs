using UnityEngine;

// (2026-11-26) 진화 = MinionEvolutionSetting + EvolutionMinion/Behaviours (예전 [각성] 효과 삭제)
public class PierceMinion : Minion
{
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

        AllyBulletObject bulletObj = pierceBulletSpec.Instantiate(this);
        bulletObj.transform.localScale = Vector3.one;
        return bulletObj;
    }


    // public override string GetDescription(int level =0)
    // {
    //     return "관통탄을 발사합니다";
    // }
}
using UnityEngine;

//엉뚱한 슬라임 
public class WackySlime : SlimeGrowth1
{
    public WackySlimeData wackySlimeData;
    float fireTimer;
    AllyBulletSpec allyBulletSpec;
    public float wackyFireSpeed;
    
    public int skillBulletCount;
    public float skillCoolTime;
    public override void Spawn(Vector2 pos, int lv)
    {
        wackySlimeData = SlimeManager.Instance.GetSlimeData(key) as WackySlimeData;
        base.Spawn(pos, lv);      
    }

    public override void InitSlime()
    {
        skillCoolTime = float.Parse(wackySlimeData.GetUniqueStat(SlimeStatType.SkillCooltime).values[mergeLevel]) ;
        allyBulletSpec = new AllyBulletSpec();
        fireTimer = 0;
        wackyFireSpeed = 1;

        base.InitSlime();
    }

    public override void Update()
    {
        base.Update();
        if (fireTimer >= skillCoolTime)
        {
            Fire();
            fireTimer = 0;
        }
        fireTimer += Time.deltaTime * wackyFireSpeed;
    }

    void Fire()
    {
        for (int i = 0; i < mergeLevel + 1; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Fire(randomDir);
        }   
    }

    public override void Fire(Vector2 dir)
    { 
        if(skillCounter >= 6)
        {
            ExecuteSkill();
        }
        else
            base.Fire(dir);
    }
    
    int skillCounter;
    public void CountUp()
    {
        skillCounter++;
    }
    public void ExecuteSkill()
    {
        float baseAngle = Vector2.SignedAngle(Vector2.right, AttackDirecton());
        float angleStep = 360f / skillBulletCount;
        for (int i = 0; i < skillBulletCount; i++)
        {
            float rad = (baseAngle + i * angleStep) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            base.Fire(dir);
        }
        skillCounter = 0;
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;

        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }

    // public override string GetDescription(int level =0)
    // {
    //     return string.Format("{0}초마다 랜덤한 방향으로 총알을 발사합니다.",wackySlimeData.fireTimes[level]);
    // }

}
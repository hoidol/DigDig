using UnityEngine;

// 공격력 랜덤 폭: 데이터에는 중간값만 (MinionData.attackPowerRandomRatio)
// [각성] (진화) EVOLVE_SKILL_INTERVAL번째 공격마다 EVOLVE_SKILL_BULLET_COUNT방향으로 원형 발사
public class RandomMinion : MinionGrowth1
{
    public const int EVOLVE_SKILL_INTERVAL = 6;
    public const int EVOLVE_SKILL_BULLET_COUNT = 4;

    public RandomMinionData randomMinionData;
    AllyBulletSpec allyBulletSpec;
    int attackCount;

    public override void Spawn(Vector2 pos, int lv)
    {
        randomMinionData = MinionManager.Instance.GetMinionData(key) as RandomMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        allyBulletSpec = new AllyBulletSpec();
        attackCount = 0;
        base.InitMinion();
    }

    public override void Fire(Vector2 dir)
    {
        if (dir == Vector2.zero)
            return;
        if (!isEvolved || (attackCount + 1) % EVOLVE_SKILL_INTERVAL != 0)
        {
            base.Fire(dir);
            if (attackTimer == 0) // 발사에 성공했을 때만 횟수 증가
                attackCount++;
            return;
        }

        float baseAngle = Random.Range(0f, 360f);
        float angleStep = 360f / EVOLVE_SKILL_BULLET_COUNT;
        for (int i = 0; i < EVOLVE_SKILL_BULLET_COUNT; i++)
        {
            float rad = (baseAngle + i * angleStep) * Mathf.Deg2Rad;
            FireBullet(new Vector2(Mathf.Cos(rad), Mathf.Sin(rad))); // 원형 방향 그대로 (멀티 타겟 X)
        }
        attackTimer = 0;
        attackCount++;
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;

        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }
}

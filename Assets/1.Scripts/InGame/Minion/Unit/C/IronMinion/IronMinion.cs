using UnityEngine;

// 강철 슬라임: 일반 탄 발사, 엘리트/보스에게 데미지 ×eliteBossDamageMul
// 배율은 탄 스펙의 BigEnemyDamageForce로 보냄 → 연발 타일로 다시 쏜 탄에도 유지. 거인 사냥 타일과는 곱연산
// [각성] (진화) 엘리트/보스 배율 +EVOLVE_ELITE_BOSS_MUL_ADD
public class IronMinion : MinionGrowth1
{
    public const float EVOLVE_ELITE_BOSS_MUL_ADD = 0.2f;
    IronMinionData ironMinionData;
    AllyBulletSpec allyBulletSpec;
    readonly BigEnemyDamageForce bigEnemyDamageForce = new BigEnemyDamageForce(1f); // 탄마다 새로 만들지 않고 mul만 갱신
    public float eliteBossDamageMul;

    public override void Spawn(Vector2 pos, int lv)
    {
        ironMinionData = MinionManager.Instance.GetMinionData(key) as IronMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        allyBulletSpec = new AllyBulletSpec();
        eliteBossDamageMul = ironMinionData.eliteBossDamageMuls[mergeLevel];
        base.InitMinion();
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec(); // forces를 비우므로 그 뒤에 추가
        bigEnemyDamageForce.mul = eliteBossDamageMul + (isEvolved ? EVOLVE_ELITE_BOSS_MUL_ADD : 0f);
        allyBulletSpec.AddBulletForce(bigEnemyDamageForce);
        return allyBulletSpec.Instantiate(this);
    }
}

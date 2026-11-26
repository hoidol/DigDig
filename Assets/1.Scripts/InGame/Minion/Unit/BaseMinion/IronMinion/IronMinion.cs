using UnityEngine;

// 강철 슬라임: 일반 탄 발사, 엘리트/보스에게 데미지 ×eliteBossDamageMul
// 배율은 탄 스펙의 BigEnemyDamageForce로 보냄 → 연발 타일로 다시 쏜 탄에도 유지. 거인 사냥 타일과는 곱연산
// (2026-11-26) 진화 = MinionEvolutionSetting + EvolutionMinion/Behaviours (예전 [각성] 효과 삭제)
// 진화하면 엘리트·보스 배율 = IronEvolution.EliteBossDamageMul (Setting iron.eliteBossDamageMul)
// (2026-11-26) 일반 적에게 맞으면 stunChances 확률로 stunTimes초 스턴 (엘리트·보스는 안 걸림, 머지 레벨별)
public class IronMinion : Minion
{
    IronMinionData ironMinionData;
    AllyBulletSpec allyBulletSpec;
    readonly BigEnemyDamageForce bigEnemyDamageForce = new BigEnemyDamageForce(1f); // 탄마다 새로 만들지 않고 mul만 갱신
    readonly NormalEnemyStunBehavior stunBehavior = new NormalEnemyStunBehavior(); // 탄마다 새로 만들지 않고 값만 갱신
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
        bigEnemyDamageForce.mul = EvolutionAbility is IronEvolution evo && evo.EliteBossDamageMul > 0f ? evo.EliteBossDamageMul : eliteBossDamageMul;
        allyBulletSpec.AddBulletForce(bigEnemyDamageForce);
        stunBehavior.chance = ironMinionData.stunChances[mergeLevel];
        stunBehavior.duration = ironMinionData.stunTimes[mergeLevel];
        allyBulletSpec.AddBulletBehaviour(stunBehavior);
        return allyBulletSpec.Instantiate(this);
    }
}

// (2026-11-26) 일반 적(EnemyType.Normal)에게만 chance(0~1) 확률로 duration초 스턴
public class NormalEnemyStunBehavior : IBulletBehavior
{
    public int ApplyOrder => -1; // 관통 등 다음 처리를 끊는 behavior보다 먼저
    public float chance;
    public float duration;

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (hit.CurHp <= 0)
            return true;
        if (hit is Enemy enemy && enemy.enemyType == EnemyType.Normal && Random.value < chance)
            hit.ApplyStatusEffect(new StunEffect(duration));
        return true;
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 강철 슬라임: 일반 탄 발사, 엘리트/보스에게 데미지 ×eliteBossDamageMul
// 배율은 탄 스펙의 BigEnemyDamageForce로 보냄 → 연발 타일로 다시 쏜 탄에도 유지. 거인 사냥 타일과는 곱연산
public class IronSlime : SlimeGrowth1
{
    IronSlimeData ironSlimeData;
    AllyBulletSpec allyBulletSpec;
    readonly BigEnemyDamageForce bigEnemyDamageForce = new BigEnemyDamageForce(1f); // 탄마다 새로 만들지 않고 mul만 갱신
    public float eliteBossDamageMul;
    public IronEnhance5Ability eliteBossTargetAbility; // [각성] 엘리트/보스 우선 공격

    public override void Spawn(Vector2 pos, int lv)
    {
        ironSlimeData = SlimeManager.Instance.GetSlimeData(key) as IronSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        allyBulletSpec = new AllyBulletSpec();
        eliteBossDamageMul = ironSlimeData.eliteBossDamageMuls[mergeLevel];
        base.InitSlime(); // 강화 능력이 eliteBossDamageMul을 올릴 수 있으므로 기본값 설정 후 호출
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec(); // forces를 비우므로 그 뒤에 추가
        bigEnemyDamageForce.mul = eliteBossDamageMul;
        allyBulletSpec.AddBulletForce(bigEnemyDamageForce);
        return allyBulletSpec.Instantiate(this);
    }

    public override Transform FindTarget()
    {
        if (eliteBossTargetAbility == null || !eliteBossTargetAbility.isActivate)
            return base.FindTarget();
        return FindTargets(1).FirstOrDefault();
    }

    public override List<Transform> FindTargets(int count)
    {
        if (eliteBossTargetAbility == null || !eliteBossTargetAbility.isActivate)
            return base.FindTargets(count);

        // 범위 안 전체를 기존 우선순위로 정렬한 뒤 엘리트/보스를 앞으로 (안정 정렬이라 같은 그룹 안 순서는 유지)
        List<Transform> all = InGameUtil.FindTargets(transform.position, AttackRange, targetLayerMask, findTargetType, MAX_SEARCH_COUNT);
        return all.OrderByDescending(IsEliteOrBoss).Take(count).ToList();
    }

    const int MAX_SEARCH_COUNT = 64; // 범위 안 후보 최대 수 (List 용량으로 쓰이므로 int.MaxValue 금지)

    static bool IsEliteOrBoss(Transform t)
    {
        return t.GetComponent<EliteEnemy>() != null || t.GetComponent<Boss>() != null;
    }
}

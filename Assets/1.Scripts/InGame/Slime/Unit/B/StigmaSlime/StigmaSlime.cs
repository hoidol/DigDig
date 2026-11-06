using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// [B] 낙인: 일반 탄 + 적중 시 확률로 낙인 (StigmaEffect). 낙인 중첩당 그 적이 받는 모든 피해 증가
// 확률은 머지 레벨(StigmaSlimeData.stigmaChances), 중첩당 피해/최대 중첩/타겟 우선순위는 강화 능력(5/8/11)
public class StigmaSlime : SlimeGrowth1
{
    StigmaSlimeData stigmaSlimeData;
    AllyBulletSpec allyBulletSpec;

    public float stigmaChance;
    public int maxStack;
    public float damagePerStack;
    public bool targetLowStigmaFirst; // [11] 낙인이 적은 적 우선 공격

    public override void Spawn(Vector2 pos, int lv)
    {
        stigmaSlimeData = SlimeManager.Instance.GetSlimeData(key) as StigmaSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        allyBulletSpec = new AllyBulletSpec();
        stigmaChance = stigmaSlimeData.stigmaChances.GetByMergeLevel(mergeLevel);
        maxStack = stigmaSlimeData.maxStack;
        damagePerStack = stigmaSlimeData.damagePerStack;
        targetLowStigmaFirst = false;
        base.InitSlime(); // 강화 능력이 값을 바꿀 수 있으므로 기본값 설정 후 호출
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        allyBulletSpec.AddBulletBehaviour(new StigmaOnHitBehavior(stigmaChance, maxStack, damagePerStack));
        return allyBulletSpec.Instantiate(this);
    }

    public override Transform FindTarget()
    {
        if (!targetLowStigmaFirst)
            return base.FindTarget();
        return FindTargets(1).FirstOrDefault();
    }

    // [11] 기존 우선순위(findTargetType)를 유지한 채 낙인 중첩이 적은 적을 앞으로 (OrderBy는 안정 정렬)
    public override List<Transform> FindTargets(int count)
    {
        if (!targetLowStigmaFirst)
            return base.FindTargets(count);

        List<Transform> all = InGameUtil.FindTargets(transform.position, AttackRange, targetLayerMask, findTargetType, MAX_CANDIDATES);
        return all.OrderBy(GetStigmaStack).Take(count).ToList();
    }

    const int MAX_CANDIDATES = 64; // 사거리 안 후보 최대 수 (List 용량으로도 쓰이므로 int.MaxValue 금지)

    static int GetStigmaStack(Transform t)
    {
        return t.TryGetComponent(out StatusEffectHandler handler) ? handler.StigmaStack : 0;
    }
}

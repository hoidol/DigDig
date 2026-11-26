using System.Collections.Generic;
using UnityEngine;

// (2026-11-24) 5성 진화 갈래의 특수 동작. 진화할 때 미니언 자식 오브젝트에 붙음 → OnEquip, 미니언이 사라지거나 새 미니언이 되면 OnUnequip 후 삭제
// 진화해도 미니언 클래스·key는 그대로 (피해 집계·미션·합성 등 key 기준 시스템 유지). 종마다 이 클래스를 상속해서 동작을 만든다
// (2026-11-26) 수치는 MinionEvolutionSetting(Resources 에셋)의 종별 Config → 동작 클래스는 Config<T>로 읽기만
//   훅: OnBulletFired(탄을 쏠 때마다) · 발동 스킬(HasActiveSkill: 머리 위 버튼 EvolutionSkillButton → UseActiveSkill)
public abstract class MinionEvolution : MonoBehaviour
{
    protected Minion minion;
    public EvolutionConfig config;

    protected T Config<T>() where T : EvolutionConfig => config as T;

    public virtual void OnEquip(Minion owner)
    {
        minion = owner;
    }

    public virtual void OnUnequip()
    {
    }

    // 미니언이 탄 1발을 쏠 때마다 (Minion.FireBullet, 타일 룬 효과가 붙은 뒤)
    public virtual void OnBulletFired(AllyBulletObject bullet) { }

    // ---- 발동 스킬 (머리 위 버튼으로 직접 씀). 없으면 HasActiveSkill false
    public virtual bool HasActiveSkill => false;
    public virtual float ActiveSkillProgress => 0f;   // 쿨타임 0~1 (1 = 준비됨)
    public virtual bool IsActiveSkillRunning => false; // 발동 중 (버튼 잠김)
    public bool IsActiveSkillReady => HasActiveSkill && !IsActiveSkillRunning && ActiveSkillProgress >= 1f;
    public virtual bool UseActiveSkill() => false;    // 쓰면 true (대상이 없으면 false = 그대로 대기)

    // ---- 공용 도우미
    // 드래그 중이 아니고 게임 진행 중 (스킬 타이머·자동 발동 조건)
    protected bool CanAct => minion != null && !minion.IsBeingDragged
        && (GameManager.Instance == null || GameManager.Instance.isPlaying);

    // 미니언 공격 기준 피해 1번 (치명타·피해 집계 포함)
    public static void DealDamage(Minion owner, IHittable target, float damage, AttackType? attackType = null)
    {
        if (owner == null || target == null || target.CurHp <= 0)
            return;
        AllyUnitDamageData data = new AllyUnitDamageData();
        data.Init(owner);
        data.damage = damage;
        data.attackType = attackType ?? owner.AttackType;
        owner.AccumulateDamage(data.damage);
        target.TakeDamage(data);
    }

    // center 반경 radius 안 적 (가까운 순, 최대 maxCount)
    public static List<IHittable> FindEnemies(Minion owner, Vector2 center, float radius, int maxCount = 30)
    {
        var result = new List<IHittable>();
        var targets = InGameUtil.FindTargets(center, radius, owner.targetLayerMask, FindTargetType.Closest, maxCount);
        if (targets == null)
            return result;
        foreach (Transform t in targets)
            if (t != null && t.TryGetComponent(out IHittable h) && h.CurHp > 0)
                result.Add(h);
        return result;
    }

    public static void ApplyStatus(IHittable target, StatusEffect effect)
    {
        if (target is Component c && c != null && c.TryGetComponent(out StatusEffectHandler handler))
            handler.Apply(effect);
    }

    // chance(0~1) 확률로 스턴
    public static void TryStun(IHittable target, float chance, float duration)
    {
        if (chance > 0f && duration > 0f && Random.value < chance)
            ApplyStatus(target, new StunEffect(duration));
    }
}

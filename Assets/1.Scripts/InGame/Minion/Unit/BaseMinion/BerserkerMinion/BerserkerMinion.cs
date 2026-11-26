using System.Collections.Generic;
using UnityEngine;

// [A] 버서커: 근접 공격(탄 없음, 2026-11-16) — 공격 사거리(AttackRange) 안 타겟을 공격 주기마다 바로 때림
// 사거리(AttackRange) 안에서 적이 죽을 때마다 공속 중첩 (최대 maxStack). 처치 감지 범위 = 공격 사거리 (따로 없음)
// 공속 = 기본(버프 포함) × (1 + stack × attackSpeedPerStack). 마지막 처치 후 stackDuration 지나면 중첩 초기화
// 타일 아이템: 적중형(치명타·둔화·스턴 등)은 타격마다 직접 적용(회전체 OrbitOrb와 같음), 발사형(연발 등)은 탄이 없어 적용 안 됨
// (2026-11-26) 진화 = MinionEvolutionSetting + EvolutionMinion/Behaviours (예전 [각성] 효과 삭제)
public class BerserkerMinion : Minion
{
    BerserkerMinionData berserkerMinionData;

    public float attackSpeedPerStack;
    public int maxStack;
    public float stackDuration;

    public int stack;
    public float stackTimer;

    // 머리 위 바(MinionGameStatePanel) = 중첩 비율 (HasSkill TRUE라 쿨타임 바 자리에 표시)
    public override float SkillProgress => maxStack > 0 ? Mathf.Clamp01((float)stack / maxStack) : 0f;
    // 머리 위 글씨 "중첩 N" (번역 Minion_Stack)
    public override string StateText => string.Format(TranslateManager.GetText("Minion_Stack"), stack);

    public override void Spawn(Vector2 pos, int lv)
    {
        berserkerMinionData = MinionManager.Instance.GetMinionData(key) as BerserkerMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        attackSpeedPerStack = berserkerMinionData.attackSpeedPerStacks.GetByMergeLevel(mergeLevel);
        maxStack = berserkerMinionData.maxStack;
        stackDuration = berserkerMinionData.stackDuration;
        stack = 0;
        stackTimer = 0;
        base.InitMinion();
    }

    // 중첩은 버프까지 반영된 값에 곱함 (중첩이 바뀔 때마다 UpdateMinion으로 다시 계산)
    public override void UpdateMinion()
    {
        base.UpdateMinion();
        statDic[StatType.AttackSpeed] *= 1f + stack * attackSpeedPerStack;
    }

    public override AllyBulletObject GetBullet() => null; // 근접 공격 (Fire 재정의)

    // 근접 공격: 사거리 안 타겟을 바로 때림. 탄 수(다중 타겟 타일 등)만큼 서로 다른 타겟
    public override void Fire(Vector2 dir)
    {
        if (dir == Vector2.zero)
            return;

        int count = ShootCount + shotModifier.bonusShootCount;
        List<Transform> targets = count > 1 ? FindTargets(count) : new List<Transform> { targetTr };
        bool hit = false;
        foreach (Transform target in targets)
        {
            if (target == null || !target.TryGetComponent(out IHittable hittable))
                continue;
            MeleeHit(hittable, ((Vector2)(target.position - transform.position)).normalized);
            hit = true;
        }
        if (hit)
            attackTimer = 0;
    }

    // 공통 공격 소리 대신 근접 타격 소리 (공격 성공 시 Minion.Update가 1번 부름)
    public override void PlayAttackSound() => SoundManager.Play(SFXType.BerserkerHit);

    static readonly Color CLAW_COLOR = new Color(1f, 0.92f, 0.85f, 1f); // 할퀴기 색 (흰빛)

    void MeleeHit(IHittable hittable, Vector2 dir)
    {
        TileRune tileRune = shotModifier.tileRune;
        AllyUnitDamageData damageData = new AllyUnitDamageData();
        damageData.Init(this); // 치명타 확률(설의 버프 등) 반영
        damageData.damage = AttackPower;
        damageData.attackType = AttackType;
        if (tileRune != null)
            tileRune.ModifyDamageData(damageData);
        AccumulateDamage(damageData.damage);
        hittable.TakeDamage(damageData);
        // (2026-11-26) 타격 이펙트 = 킹(기사) 휘두르기를 얇게 만든 손톱 할퀴기 3줄 (이전: 공용 Hit 이펙트)
        HeroSkillVisual.PlayClaw(hittable.Transform.position, dir, CLAW_COLOR);
        IBulletBehavior hitBehavior = tileRune != null ? tileRune.HitBehavior : null; // 확률형은 읽을 때 판정하므로 한 번만 읽음
        if (hitBehavior != null)
            hitBehavior.OnHit(null, hittable, new RaycastHit2D(), dir);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    void OnEnemyDeadEvent(EnemyDeadEvent e) => TryAddStack(e.position);

    public override bool IsMelee => true; // (2026-11-16) 근접 공격 (탄 없음)

    void TryAddStack(Vector2 deadPos)
    {
        if (berserkerMinionData == null) // Spawn 전
            return;
        if (((Vector2)transform.position - deadPos).sqrMagnitude > AttackRange * AttackRange)
            return;

        stackTimer = stackDuration; // 최대 중첩이어도 유지 시간은 갱신
        if (stack >= maxStack)
            return;

        stack++;
        UpdateMinion();
    }

    public override void Update()
    {
        base.Update();
        if (stack > 0)
        {
            stackTimer -= Time.deltaTime;
            if (stackTimer <= 0)
            {
                stack = 0;
                UpdateMinion();
            }
        }
    }
}

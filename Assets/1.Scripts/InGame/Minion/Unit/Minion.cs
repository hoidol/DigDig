using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public abstract class Minion : MonoBehaviour, IAllyUnit, ITileObject, IBuffable, IShotModifiable
{
    public string key;
    public int mergeLevel;
    // 권능 쌍둥이 반지: 같은 종이 붙어 있으면 1. 데이터 스탯(공격력 등)만 이만큼 높은 머지 레벨 값으로 (합성·표시는 mergeLevel 그대로)
    public int twinBonus;
    int StatMergeLevel => Mathf.Min(mergeLevel + twinBonus, MinionData.MAX_MERGE_LEVEL);

    public void SetTwinBonus(int bonus)
    {
        if (twinBonus == bonus)
            return;
        twinBonus = bonus;
        if (minionData != null)
            UpdateMinion();
    }

    // 진화 (EvolutionTileItem: 최대 머지 레벨 슬라임이 올라가면 진화) = 슬라임 고유 [각성] 능력 발현
    // 각성 효과는 슬라임마다 isEvolved일 때만 동작. 진화 순간에 바꿔야 하는 값(개수 등)은 OnEvolve에서 적용
    // 합성·교체로 새로 생긴 슬라임은 진화 안 된 상태 (Spawn에서 초기화)
    public bool isEvolved;
    public virtual bool CanEvolve => mergeLevel >= MinionData.MAX_MERGE_LEVEL && !isEvolved;
    public void Evolve()
    {
        if (!CanEvolve)
            return;
        isEvolved = true;
        Debug.Log($"[Evolve] {key} 진화 (머지 레벨 {mergeLevel})");
        OnEvolve();
        UpdateMinion();
        SoundManager.Play(SFXType.Evolve);
    }

    protected virtual void OnEvolve()
    {
    }

    // public MinionMovement movement;

    public Dictionary<StatType, float> statDic = new Dictionary<StatType, float>();

    // attackPowerRandomRatio > 0이면 매 공격마다 중간값 ±비율 범위 랜덤
    public virtual float AttackPower => MinionData.ApplyRandomRatio(statDic[StatType.AttackPower], minionData.attackPowerRandomRatio);
    public float AttackSpeed => statDic[StatType.AttackSpeed];
    public float AttackRange => statDic[StatType.AttackRange];
    public float CritChance => statDic[StatType.CritChance]; // % 단위. 기본값은 MinionData.csv CritChance, 버프(설의 등)로 오름

    public Transform rootTr;
    public MinionData MinionData => MinionManager.Instance.GetMinionData(key);
    public float AccumulatedDamage { get; set; }

    public float attackTimer;
    //적 찾는 방식 설정
    public FindTargetType findTargetType = FindTargetType.Closest;

    public FindTargetType FindTargetType => findTargetType;
    // 타겟 방식 설정이 의미 있는지 (회전체·경로 순환처럼 타겟을 노리지 않는 슬라임은 false → 설정 버튼 숨김)
    public virtual bool UseFindTarget => true;

    public void AccumulateDamage(float d)
    {
        AccumulatedDamage += d;
    }
    public Transform Transform => transform;
    public string Key => key;
    public AllyType AllyType => AllyType.Minion;
    public AttackType AttackType => minionData.attackType;
    public LayerMask targetLayerMask = 1 << 3;


    public Action<Transform> onTargetListener;
    public MinionData minionData;
    public List<Buff> activeBuffs = new List<Buff>();
    public ShotModifier shotModifier = new ShotModifier(); // 타일 아이템의 발사 방식 효과
    public ShotModifier ShotModifier => shotModifier;
    // 몸 애니메이션 (Root/BodyRoot의 Animator, 컨트롤러 5.Animation/Minion/Minion.controller): Idle 기본, Attack·Skill 트리거
    static readonly int ANIM_ATTACK = Animator.StringToHash("Attack");
    static readonly int ANIM_SKILL = Animator.StringToHash("Skill");
    protected Animator bodyAnimator;

    public void PlayAttackAnim() => SetAnimTrigger(ANIM_ATTACK);
    // 공격 소리: MinionData.attackSfx, 클립이 없으면 공통 SFXType.MinionAttack
    public virtual void PlayAttackSound()
    {
        SoundManager soundManager = SoundManager.Instance;
        if (soundManager != null)
            soundManager.PlaySound(minionData != null ? minionData.attackSfx : null, SFXType.MinionAttack);
    }
    public void PlaySkillAnim() => SetAnimTrigger(ANIM_SKILL);

    // 스킬로 강화되는 동안(공속 버프 등) 미니언을 따라다니는 이펙트 (Resources/Effect/PowerUpEffect, EffectType.PowerUp)
    ParticleEffect powerUpEffect;
    public void PlayPowerUpEffect(float duration)
    {
        StopPowerUpEffect();
        SoundManager.Play(SFXType.PowerUp);
        if (EffectManager.Instance == null)
            return;
        powerUpEffect = EffectManager.Instance.GetEffect(EffectType.PowerUp) as ParticleEffect;
        if (powerUpEffect != null)
            powerUpEffect.PlayFollow(transform, duration);
    }
    public void StopPowerUpEffect()
    {
        // 이미 끝나서 다른 미니언이 재사용 중이면 건드리지 않음
        if (powerUpEffect != null && powerUpEffect.FollowTarget == transform)
            powerUpEffect.EndEffect();
        powerUpEffect = null;
    }

    // 받은 버프의 이펙트 (Buff.effectType). activeBuffs 기준으로 켜고 끔 → 버프가 빠지면 이펙트도 같이 꺼짐
    readonly BuffEffectDisplay buffEffectDisplay = new BuffEffectDisplay();
    void RefreshBuffEffects() => buffEffectDisplay.Refresh(transform, activeBuffs, isActiveAndEnabled);
    void ClearBuffEffects() => buffEffectDisplay.Clear(transform);

    void SetAnimTrigger(int hash)
    {
        if (bodyAnimator != null && bodyAnimator.isActiveAndEnabled && bodyAnimator.runtimeAnimatorController != null)
            bodyAnimator.SetTrigger(hash);
    }

    public virtual void Awake()
    {
        if (rootTr == null)
            rootTr = transform.Find("Root");
        if (rootTr != null)
        {
            bodyAnimator = rootTr.GetComponentInChildren<Animator>(true);
            rootBaseLocalPos = rootTr.localPosition;
        }
        if (bodyAnimator != null)
            bodyRootBaseScale = bodyAnimator.transform.localScale;
    }

    // ---- 등장 연출 (2026-11-16): 살짝 위에서 떨어져 착지하며 찌그러졌다 돌아옴 ----
    // 미니언 위치(transform)가 아니라 몸(Root)만 움직임 → 칸 이동·드래그와 겹쳐도 어긋나지 않음
    const float SPAWN_DROP_HEIGHT = 0.6f;
    const float SPAWN_DROP_TIME = 0.25f;
    const float SPAWN_SQUASH_TIME = 0.2f;
    static readonly Vector3 SPAWN_SQUASH_SCALE = new Vector3(1.25f, 0.75f, 1f);
    Vector3 rootBaseLocalPos;
    Vector3 bodyRootBaseScale = Vector3.one;
    Sequence spawnSeq;

    void PlaySpawnDrop()
    {
        if (rootTr == null)
            return;
        StopSpawnDrop();

        Transform bodyRoot = bodyAnimator != null ? bodyAnimator.transform : null;
        rootTr.localPosition = rootBaseLocalPos + Vector3.up * SPAWN_DROP_HEIGHT;

        spawnSeq = DOTween.Sequence().SetLink(gameObject);
        spawnSeq.Append(rootTr.DOLocalMove(rootBaseLocalPos, SPAWN_DROP_TIME).SetEase(Ease.InQuad));
        AppendLandSquash(bodyRoot);
    }

    // 머지 결과: 제자리에서 살짝 뛰어올랐다 내려와 착지 (Spawn의 떨어지는 연출 대신)
    const float MERGE_HOP_HEIGHT = 0.35f;
    const float MERGE_HOP_UP_TIME = 0.15f;
    const float MERGE_HOP_DOWN_TIME = 0.15f;

    public void PlayMergeHop()
    {
        if (rootTr == null)
            return;
        StopSpawnDrop();

        Transform bodyRoot = bodyAnimator != null ? bodyAnimator.transform : null;
        spawnSeq = DOTween.Sequence().SetLink(gameObject);
        spawnSeq.Append(rootTr.DOLocalMove(rootBaseLocalPos + Vector3.up * MERGE_HOP_HEIGHT, MERGE_HOP_UP_TIME).SetEase(Ease.OutQuad));
        spawnSeq.Append(rootTr.DOLocalMove(rootBaseLocalPos, MERGE_HOP_DOWN_TIME).SetEase(Ease.InQuad));
        AppendLandSquash(bodyRoot);
    }

    // 착지: 납작해졌다가 통통 튀며 원래 크기
    void AppendLandSquash(Transform bodyRoot)
    {
        if (bodyRoot == null)
            return;
        spawnSeq.Append(bodyRoot.DOScale(Vector3.Scale(bodyRootBaseScale, SPAWN_SQUASH_SCALE), SPAWN_SQUASH_TIME * 0.35f).SetEase(Ease.OutQuad));
        spawnSeq.Append(bodyRoot.DOScale(bodyRootBaseScale, SPAWN_SQUASH_TIME * 0.65f).SetEase(Ease.OutBack));
    }

    void StopSpawnDrop()
    {
        if (spawnSeq != null && spawnSeq.IsActive())
            spawnSeq.Kill();
        spawnSeq = null;
        if (rootTr != null)
            rootTr.localPosition = rootBaseLocalPos;
        if (bodyAnimator != null)
            bodyAnimator.transform.localScale = bodyRootBaseScale;
    }
    public UserMinion userMinion;
    public MinionEnhanceInfo minionEnhanceInfo;
    public Transform rangeTr;
    public virtual void Spawn(Vector2 pos, int mLv)
    {
        userMinion = UserDataManager.Instance.userMinionManager.GetUserMinion(key);

        minionData = MinionManager.Instance.GetMinionData(key);
        minionEnhanceInfo = minionData.GetCommonMinionEnhanceInfo(userMinion.EnhanceLevel);
        transform.position = pos;
        this.mergeLevel = mLv;
        twinBonus = 0; // 풀링 재사용
        isDraging = false;
        isEvolved = false; // 풀링 재사용 · 합성/교체로 새로 생긴 슬라임은 진화 안 된 상태

        statDic.Clear();
        statDic.Add(StatType.AttackPower, 0);
        statDic.Add(StatType.AttackSpeed, 0);
        statDic.Add(StatType.AttackRange, 0);
        statDic.Add(StatType.CritChance, 0);
        rangeTr.gameObject.SetActive(false);

        InitMinion();
        UpdateMinion();

        // 머리 위 상태 표시 (머지 레벨 / 스킬 쿨타임). 씬에 GameWorldCanvas가 없으면 생략
        GameWorldCanvas worldCanvas = GameWorldCanvas.Instance;
        if (worldCanvas != null)
            worldCanvas.AttachMinionState(this);
        PlaySpawnDrop();
    }

    // 머리 위 상태 표시(MinionGameStatePanel) 위치. 프리팹에서 지정, 비워 두면 미니언 위치 + DEFAULT_STATE_POINT_OFFSET
    public const float DEFAULT_STATE_POINT_OFFSET = 0.9f;
    [SerializeField] protected Transform statePoint;
    public Vector3 StatePointPosition => statePoint != null ? statePoint.position : transform.position + Vector3.up * DEFAULT_STATE_POINT_OFFSET;

    // 쿨타임 스킬 진행도 0~1 (1 = 사용 가능). MinionData.hasSkill인 미니언이 재정의
    public virtual float SkillProgress => 0f;

    // 머리 위 추가 상태 글씨 (MinionGameStatePanel.stateText). null이면 숨김 (버서커 "중첩 N" 등)
    public virtual string StateText => null;

    // 테스트 창(Tools/Test/Minion Test)용: 스킬 쿨타임을 바로 채움. 시간형 = 다 참, 횟수형 = 1회 남김(다음 처치·공격에 발동)
    public virtual void FillSkillForTest() { }

    // ---- 스킬 공용 도우미 (2026-11-15) ----
    // 사거리 안 가장 가까운 적 (없으면 null)
    protected Transform FindClosestTargetInRange()
    {
        var targets = InGameUtil.FindTargets(transform.position, AttackRange, targetLayerMask, FindTargetType.Closest, 1);
        return targets != null && targets.Count > 0 ? targets[0] : null;
    }

    // baseAngle(도)부터 count방향 같은 간격으로 1발씩 (멀티 타겟 X)
    protected void FireCircle(float baseAngle, int count)
    {
        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float rad = (baseAngle + i * angleStep) * Mathf.Deg2Rad;
            FireBullet(new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)));
        }
    }

    // 첫 방향 = target 쪽
    protected void FireCircleAt(Transform target, int count)
        => FireCircle(Vector2.SignedAngle(Vector2.right, target.position - transform.position), count);

    public virtual void InitMinion()
    {

    }

    public virtual void InitMinionStat()
    {
        statDic[StatType.AttackPower] = minionData.GetMinionStat(StatType.AttackPower, userMinion.EnhanceLevel).GetValue<float>(StatMergeLevel);
        statDic[StatType.AttackSpeed] = GetDataStat(StatType.AttackSpeed); // MinionData.csv (강화·머지 레벨 무관)
        statDic[StatType.AttackRange] = GetDataStat(StatType.AttackRange);
        statDic[StatType.CritChance] = GetDataStat(StatType.CritChance); // % 단위. MinionData.csv (기본 0)
    }

    // 데이터 스탯 값 (없으면 0)
    float GetDataStat(StatType statType)
    {
        MinionStat stat = minionData.GetMinionStat(statType, userMinion.EnhanceLevel);
        return stat != null ? stat.GetValue<float>(StatMergeLevel) : 0f;
    }

    public virtual void UpdateMinion()
    {
        InitMinionStat();
        UpdateReinforce();
        
        foreach (var buff in activeBuffs)
        {
            statDic.TryGetValue(buff.statType, out float cur); // 없는 스탯 버프도 예외 없이 (기본 0)
            statDic[buff.statType] = buff.Apply(cur);
        }

        // 권능 왕의 인장: 조건 충족 시 모든 슬라임 공속
        KingSealAuthority kingSeal = AuthoritySystem.Get<KingSealAuthority>();
        if (kingSeal != null && kingSeal.active)
            statDic[StatType.AttackSpeed] *= KingSealAuthority.ATTACK_SPEED_MUL;

        // 사거리 버프까지 반영된 값으로 표시
        rangeTr.localScale = Vector3.one * statDic[StatType.AttackRange];
    }

    public virtual void OnEnable()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
        GameEventBus.Subscribe<MinionLevelUpReinforceEvent>(OnMinionLevelUpReinforceEvent);
    }

    public virtual void OnDisable()
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
        GameEventBus.Unsubscribe<MinionLevelUpReinforceEvent>(OnMinionLevelUpReinforceEvent);
        StopPowerUpEffect();
        ClearBuffEffects();
        StopSpawnDrop(); // 풀 반납 시 몸 위치·크기 원래대로

        GameWorldCanvas worldCanvas = GameWorldCanvas.Instance;
        if (worldCanvas != null)
            worldCanvas.ReleaseMinionState(this);
    }

    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        CheckTarget(e.enemy);
    }

    void OnMinionLevelUpReinforceEvent(MinionLevelUpReinforceEvent e)
    {
        if (minionData.key == e.key)
        {
            UpdateMinion();            
        }
    }

    void CheckTarget(IHittable hittable)
    {
        if (targetTr != null)
        {
            if (hittable.Transform == targetTr)
            {
                targetTr = null;
            }
        }
    }

    // 인게임 직접 강화(종 단위): 레벨당 공격력 +reinforceAttackPowerPercent% (합연산, 버프 적용 전)
    // 시각적 변화(탄 수, 체인 수 등)는 강화가 아니라 머지 레벨로 준다
    public virtual void UpdateReinforce()
    {
        if (KingSlime.Instance == null) // 씬 종료 중 (킹이 먼저 파괴됨)
            return;
        int reinforceLevel = KingSlime.Instance.statMgr.GetReinforceLevel(minionData.key);
        statDic[StatType.AttackPower] *= 1f + minionData.reinforceAttackPowerPercent / 100f * reinforceLevel;
    }

    public abstract AllyBulletObject GetBullet();
    public virtual void Update()
    {
        if (AttackSpeed >= 0)//&& !isDraging
        {
            attackTimer += Time.deltaTime * AttackSpeed / GameSetting.ATTACK_SPEED_PER_SECOND;
            if (attackTimer >= 1)
            {
                // 랜덤 타겟 차례면 이번 공격만 타겟 방식을 Random으로 (하위 클래스 FindTarget(s)도 그대로 적용됨)
                FindTargetType prevType = findTargetType;
                if (shotModifier.IsRandomShotTurn)
                    findTargetType = FindTargetType.Random;

                Fire(AttackDirecton());
                findTargetType = prevType;

                if (attackTimer == 0) // 발사에 성공해야 타이머가 리셋됨
                {
                    shotModifier.CountAttack();
                    if (UseFindTarget) // 회전체·경로 순환(타겟을 노리지 않음)은 공격 모션·소리 없음
                    {
                        PlayAttackAnim();
                        PlayAttackSound();
                    }
                }
            }
        }

        // 타겟이 없으면 마지막으로 노린 방향 유지 (타겟을 노리지 않는 미니언은 방향 고정)
        if (targetTr == null && UseFindTarget)
        {
            rootTr.localScale = new Vector3(fireDir.x >= 0 ? 1 : -1, 1, 1);
        }
    }


    public Transform targetTr;
    Vector2 fireDir; // 마지막으로 노린 방향 (바라보는 방향 유지용)
    public virtual Vector2 AttackDirecton()
    {
        Vector2 dir = Vector2.zero;

        targetTr = FindTarget();
        onTargetListener?.Invoke(targetTr);

        if (targetTr != null)
        {
            dir = (targetTr.position - transform.position).normalized;
            fireDir = dir;
            if (UseFindTarget) // 타겟을 노리지 않는 미니언(회전체 등)은 적 쪽으로 돌지 않음
                rootTr.localScale = new Vector3(dir.x >= 0 ? 1 : -1, 1, 1);
        }

        return dir;
    }


    // 한 번 공격에 발사하는 탄 수 (2 이상이면 서로 다른 타겟을 노림)
    public virtual int ShootCount => 1;

    // 서로 다른 타겟 count개 (우선순위 순). 기본은 findTargetType 기준
    public virtual List<Transform> FindTargets(int count)
    {
        // 권능 사냥꾼의 부적: 강타 슬라임은 엘리트·보스 먼저
        if (AuthoritySystem.Has<HunterCharmAuthority>() && HunterCharmAuthority.IsHeavy(MinionData))
            return HunterCharmAuthority.PreferEliteBoss(InGameUtil.FindTargets(transform.position, AttackRange, targetLayerMask, findTargetType, HunterCharmAuthority.MAX_CANDIDATES), count);
        return InGameUtil.FindTargets(transform.position, AttackRange, targetLayerMask, findTargetType, count);
    }

    public virtual void Fire(Vector2 dir)
    {
        if (dir == Vector2.zero)
            return;

        int count = ShootCount + shotModifier.bonusShootCount;
        List<Transform> targets = count > 1 ? FindTargets(count) : null;
        List<Vector2> dirs = new List<Vector2>();
        if (targets == null || targets.Count == 0)
        {
            dirs.Add(dir);
        }
        else
        {
            // 탄마다 서로 다른 타겟. 타겟이 발사 수보다 적으면 남는 탄은 쏘지 않음 (같은 적을 다시 노리지 않음)
            for (int i = 0; i < Mathf.Min(count, targets.Count); i++)
            {
                Transform target = targets[i];
                dirs.Add(((Vector2)(target.position - transform.position)).normalized);
            }
        }

        bool fired = false;
        foreach (Vector2 d in dirs)
            fired |= FireBullet(d);

        if (fired)
            attackTimer = 0;
    }

    // 지정 방향으로 1발 (멀티샷/타겟 탐색 없이)
    protected bool FireBullet(Vector2 dir)
    {
        AllyBulletObject baseBullet = GetBullet();
        if (baseBullet == null)
            return false;

        baseBullet.transform.position = transform.position;
        baseBullet.Shoot(dir, AttackPower);
        if (shotModifier.tileItem != null)
            shotModifier.tileItem.OnFired(baseBullet);
        return true;
    }

    // 일반 합성 조건: 같은 key + 같은 mergeLv, 최대 머지 레벨 미만
    public static bool CanMerge(string key1, int lv1, int growth1, string key2, int lv2, int growth2)
    {
        if (growth1 > 1 || growth2 > 1)
            return false;
        if (growth1 != growth2)
            return false;
        if (lv1 != lv2)
            return false;
        if (key1 != key2)
            return false;
        if (lv1 >= MinionData.MAX_MERGE_LEVEL || lv2 >= MinionData.MAX_MERGE_LEVEL)
            return false;
        return true;
    }

    public virtual async UniTask<(bool, string, int)> Merge(Minion target)
    {
        if (!CanMerge(key, mergeLevel, MinionData.growth, target.key, target.mergeLevel, target.MinionData.growth))
            return (false, null, 0);

        string pickedMinionKey = null;
        int lv = 0;
        if (MinionData.growth == 1)
        {
            UserMinion userMinion = UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions[Random.Range(0, UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions.Length)];
            pickedMinionKey = userMinion.key;
            if (AuthoritySystem.Has<BloodlineFlaskAuthority>())
                pickedMinionKey = BloodlineFlaskAuthority.PickResultKey(key, pickedMinionKey, Random.value);
            lv = mergeLevel + 1;
        }
        else if (MinionData.growth == 0)
        {
            UserMinion userMinion = UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions[Random.Range(0, UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions.Length)];
            pickedMinionKey = userMinion.key;
        }

        return (true, pickedMinionKey, lv);
    }
    public void SetFindTargetType(FindTargetType type)
    {
        findTargetType = type;
    }
    public virtual Transform FindTarget()
    {
        if (AuthoritySystem.Has<HunterCharmAuthority>() && HunterCharmAuthority.IsHeavy(MinionData))
            return FindTargets(1).FirstOrDefault();
        return InGameUtil.FindTarget(transform.position, AttackRange, targetLayerMask, findTargetType);
    }


    public void AddBuff(Buff buff)
    {
        activeBuffs.Add(buff);
        UpdateMinion();
        RefreshBuffEffects();
    }

    public void RemoveBuff(Buff buff)
    {
        activeBuffs.Remove(buff);
        UpdateMinion();
        RefreshBuffEffects();
    }
    bool isDraging;
    public virtual void StartDrag()
    {
        isDraging = true;
    }
    public virtual void StartTarget()
    {
        isDraging = true;
        rangeTr.gameObject.SetActive(true);
    }

    public virtual void EndTarget()
    {
        rangeTr.gameObject.SetActive(false);
    }

    public virtual void EndDrag(Tile t)
    {
        isDraging = false;
        // rangeTr.gameObject.SetActive(false);
    }
}

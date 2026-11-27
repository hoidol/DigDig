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

    // (2026-11-24) 5성 진화: 5성(MAX_MERGE_LEVEL)이 되면(Spawn) 진화 갈래 선택 창(MinionEvolutionCanvas) → 고른 갈래로 Evolve
    // 진화해도 클래스·key는 그대로: 갈래(MinionEvolutionData) 스탯 배율 + 그림 교체(MinionData.evolvedSprite) + 갈래 특수 동작(MinionEvolution 부품)
    // 종별 [각성] 효과(isEvolved일 때만 동작, OnEvolve)도 같이 켜짐. 합성·교체로 새로 생긴 미니언은 진화 안 된 상태 (Spawn에서 초기화)
    public bool isEvolved;
    public MinionEvolutionData evolution; // 고른 진화 갈래 (null = 미진화)
    MinionEvolution evolutionAbility;     // 갈래 특수 동작 (Setting Config의 동작 클래스 또는 갈래 prefab)
    public MinionEvolution EvolutionAbility => evolutionAbility;
    public virtual bool CanEvolve => mergeLevel >= MinionData.MAX_MERGE_LEVEL && !isEvolved;

    // 갈래를 고르지 않고 진화 (진화 타일·테스트 창): 첫 갈래
    public void Evolve()
    {
        MinionData data = minionData != null ? minionData : MinionData;
        var branches = data.GetEvolutions();
        Evolve(branches.Count > 0 ? branches[0] : null);
    }

    public void Evolve(MinionEvolutionData branch)
    {
        if (!CanEvolve)
            return;
        isEvolved = true;
        evolution = branch;
        GameResultStats.RecordEvolve(); // (2026-11-27) 결과 화면: 진화 수
        Debug.Log($"[Evolve] {key} 진화 (머지 레벨 {mergeLevel}, 갈래 {branch?.key})");
        ApplyEvolvedLook(true);
        if (branch != null && branch.config != null && branch.config.BehaviourType != null) // (2026-11-26) Setting 수치 + 동작 클래스
        {
            var go = new GameObject($"Evolution_{key}");
            go.transform.SetParent(transform, false);
            evolutionAbility = (MinionEvolution)go.AddComponent(branch.config.BehaviourType);
            evolutionAbility.config = branch.config;
            evolutionAbility.OnEquip(this);
        }
        else if (branch != null && branch.prefab != null)
        {
            evolutionAbility = Instantiate(branch.prefab, transform);
            evolutionAbility.OnEquip(this);
        }
        OnEvolve();
        UpdateMinion();
        SoundManager.Play(SFXType.Evolve);
        EffectManager.PlayPopText(StatePointPosition + Vector3.up * 0.4f, PopTextType.Evolve);
    }

    void ClearEvolution()
    {
        if (evolutionAbility != null)
        {
            evolutionAbility.OnUnequip();
            Destroy(evolutionAbility.gameObject);
        }
        evolutionAbility = null;
        evolution = null;
        isEvolved = false;
        ApplyEvolvedLook(false);
    }

    // 진화 모습: 몸 그림 = MinionData.evolvedSprite(2.Sprites/Minion/Growth2/{Key}, 없으면 그대로) + 몸 크기 EVOLVED_BODY_SCALE배
    public const float EVOLVED_BODY_SCALE = 1.15f;
    SpriteRenderer bodyRenderer;
    Sprite baseBodySprite;
    Vector3 bodyRootOriginScale = Vector3.one;

    void ApplyEvolvedLook(bool evolved)
    {
        StopSpawnDrop(); // 등장·머지 연출이 끝 크기를 예전 값으로 되돌리지 않게
        Sprite evolvedSprite = minionData != null ? minionData.evolvedSprite : null;
        if (bodyRenderer != null)
            bodyRenderer.sprite = evolved && evolvedSprite != null ? evolvedSprite : baseBodySprite;
        bodyRootBaseScale = bodyRootOriginScale * (evolved ? EVOLVED_BODY_SCALE : 1f);
        if (bodyAnimator != null)
            bodyAnimator.transform.localScale = bodyRootBaseScale;
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
    public float EliteBossDamage => statDic.TryGetValue(StatType.EliteBossDamage, out float v) ? v : 0f; // 엘리트·보스 피해 +% (버프로만, 드워프 종족 등)
    // (2026-11-21) 스킬 쿨타임 충전 속도 +% (버프로만, 직업 마법사 등). 스킬 쿨타임 타이머는 Time.deltaTime 대신 SkillDeltaTime으로 채움
    public float SkillCooldownSpeed => statDic.TryGetValue(StatType.SkillCooldownSpeed, out float v) ? v : 0f;
    // 이번 프레임 스킬 쿨타임 충전량 = 시간 × 충전 속도 + 즉시 단축분(AddSkillTime, 언데드 영혼 수확 등)
    // 즉시 단축분은 충전 중일 때만 쓰임: 다음 프레임까지 안 쓰이면(버프 중·대기 중) 버림
    public float SkillDeltaTime
    {
        get
        {
            // (2026-11-21) 드래그 중 · (2026-11-26) 브레이크타임 중엔 스킬 쿨타임 멈춤 (진화 발동 스킬 충전 포함)
            if (IsBeingDragged || (GameManager.Instance != null && GameManager.Instance.isBreakTime))
            {
                pendingSkillTime = 0f;
                return 0f;
            }
            float dt = Time.deltaTime * (1f + SkillCooldownSpeed / 100f);
            if (pendingSkillTime > 0f)
            {
                if (Time.frameCount - pendingSkillFrame <= 1)
                    dt += pendingSkillTime;
                pendingSkillTime = 0f;
            }
            return dt;
        }
    }
    float pendingSkillTime;
    int pendingSkillFrame;
    public void AddSkillTime(float seconds)
    {
        if (seconds <= 0f)
            return;
        pendingSkillTime += seconds;
        pendingSkillFrame = Time.frameCount;
    }
    public float CritChance => statDic[StatType.CritChance]; // % 단위. 기본값은 MinionData.csv CritChance, 버프(설의 등)로 오름

    public Transform rootTr;
    public MinionData MinionData => MinionManager.Instance.GetMinionData(key);
    public float AccumulatedDamage { get; set; }

    public float attackTimer;
    //적 찾는 방식 설정
    public FindTargetType findTargetType = FindTargetType.First;
    // (2026-11-24) 소환할 때 타겟 방식 기본값 = 가장 먼저 나온 적. 미니언마다 다르게 하려면 재정의 (얼음 = 가까운 적)
    public virtual FindTargetType DefaultFindTargetType => FindTargetType.First;

    public FindTargetType FindTargetType => findTargetType;
    // (2026-11-25) 실제로 쓰는 타겟 방식 (용사 스킬이 잠깐 바꿀 수 있음, 레인저 매의 눈. 랜덤 타겟 룬 차례는 그대로)
    public FindTargetType EffectiveFindTargetType => findTargetType != FindTargetType.Random && HeroKingAbility.ForcedAllyFindTarget.HasValue
        ? HeroKingAbility.ForcedAllyFindTarget.Value : findTargetType;
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
        bodyRootOriginScale = bodyRootBaseScale;
        if (rootTr != null)
        {
            Transform body = rootTr.Find("BodyRoot/Body");
            bodyRenderer = body != null ? body.GetComponent<SpriteRenderer>() : rootTr.GetComponentInChildren<SpriteRenderer>(true);
        }
        if (bodyRenderer != null)
            baseBodySprite = bodyRenderer.sprite;
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
    public Transform rangeTr;
    public virtual void Spawn(Vector2 pos, int mLv)
    {
        userMinion = UserDataManager.Instance.userMinionManager.GetUserMinion(key);

        minionData = MinionManager.Instance.GetMinionData(key);
        transform.position = pos;
        this.mergeLevel = mLv;
        twinBonus = 0; // 풀링 재사용
        pendingSkillTime = 0f;
        IsBeingDragged = false;
        findTargetType = DefaultFindTargetType; // 합성·교체 결과는 이후 CopyUnitOptionsFrom이 그 칸 미니언 설정으로 덮어씀
        ClearEvolution(); // 풀링 재사용 · 합성/교체로 새로 생긴 슬라임은 진화 안 된 상태

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

        // (2026-11-24) 5성이 되면 진화 갈래 선택 (합성·카드 합성 모두 여기로 옴)
        if (CanEvolve)
            MinionEvolutionCanvas.Request(this).Forget();
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
    public Transform FindClosestTargetInRange()
    {
        var targets = InGameUtil.FindTargets(transform.position, AttackRange, targetLayerMask, FindTargetType.Closest, 1);
        return targets != null && targets.Count > 0 ? targets[0] : null;
    }

    // baseAngle(도)부터 count방향 같은 간격으로 1발씩 (멀티 타겟 X)
    public void FireCircle(float baseAngle, int count)
    {
        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float rad = (baseAngle + i * angleStep) * Mathf.Deg2Rad;
            FireBullet(new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)));
        }
    }

    // 첫 방향 = target 쪽
    public void FireCircleAt(Transform target, int count)
        => FireCircle(Vector2.SignedAngle(Vector2.right, target.position - transform.position), count);

    public virtual void InitMinion()
    {

    }

    public virtual void InitMinionStat()
    {
        // MinionData.csv에 AttackPower가 비어 있으면 0 + 경고 (예외로 소환이 멈추지 않게)
        MinionStat attackStat = minionData.GetMinionStat(StatType.AttackPower);
        if (attackStat == null)
            Debug.LogWarning($"[Minion] {key} AttackPower 데이터 없음 (MinionData.csv AttackPower 열) → 0");
        statDic[StatType.AttackPower] = attackStat != null ? attackStat.GetValue<float>(StatMergeLevel) : 0f;
        statDic[StatType.AttackSpeed] = GetDataStat(StatType.AttackSpeed); // MinionData.csv (머지 레벨 무관)
        statDic[StatType.AttackRange] = GetDataStat(StatType.AttackRange);
        statDic[StatType.CritChance] = GetDataStat(StatType.CritChance); // % 단위. MinionData.csv (기본 0)
    }

    // 데이터 스탯 값 (없으면 0)
    float GetDataStat(StatType statType)
    {
        MinionStat stat = minionData.GetMinionStat(statType);
        return stat != null ? stat.GetValue<float>(StatMergeLevel) : 0f;
    }

    public virtual void UpdateMinion()
    {
        InitMinionStat();
        UpdateReinforce();
        // (2026-11-24) 5성 진화 갈래 스탯 (버프 적용 전)
        if (evolution != null)
        {
            // Setting Config면 에셋 값을 바로 (실행 중 수정 반영)
            statDic[StatType.AttackPower] *= evolution.config != null ? evolution.config.attackPowerMul : evolution.attackPowerMul;
            statDic[StatType.AttackSpeed] *= evolution.config != null ? evolution.config.attackSpeedMul : evolution.attackSpeedMul;
            statDic[StatType.AttackRange] += evolution.attackRangeAdd;
        }
        // (2026-11-24) 용사 강화: 같은 종 이웃·진화 공격력, 장착 형태가 모두 다르면 공속
        statDic[StatType.AttackPower] *= HeroUpgradeSystem.GetMinionAttackMul(this);
        statDic[StatType.AttackSpeed] *= HeroUpgradeSystem.GetMinionAttackSpeedMul(this);
        // 버프로만 오르는 스탯 → 매번 0부터 다시 (InitMinionStat이 안 채움)
        statDic[StatType.EliteBossDamage] = 0;
        statDic[StatType.DamageToLight] = 0;
        statDic[StatType.DamageToNormal] = 0;
        statDic[StatType.DamageToArmored] = 0;
        statDic[StatType.DamageToEthereal] = 0;
        statDic[StatType.SkillCooldownSpeed] = 0;
        statDic[StatType.DoubleShotChance] = 0;
        statDic[StatType.SkillCooldownSpeed] += HeroUpgradeSystem.GetMinionSkillCooldownAdd(this); // (2026-11-24) 용사 강화 정예병 스킬 쿨타임
        MinionEnhanceSystem.Apply(this, statDic); // (2026-11-25) 레벨업 미니언 강화 카드 (종 × 스탯, 강적 피해 포함 → 0으로 초기화한 뒤에)

        foreach (var buff in activeBuffs)
        {
            statDic.TryGetValue(buff.statType, out float cur); // 없는 스탯 버프도 예외 없이 (기본 0)
            statDic[buff.statType] = buff.Apply(cur);
        }

        // 사거리 버프까지 반영된 값으로 표시
        rangeTr.localScale = Vector3.one * statDic[StatType.AttackRange];
    }

    public virtual void OnEnable()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
        GameEventBus.Subscribe<MinionLevelUpReinforceEvent>(OnMinionLevelUpReinforceEvent);
        GameEventBus.Subscribe<TileObjectChangedEvent>(OnTileObjectChanged);
    }

    // (2026-11-24) 용사 강화 TwinAttack: 칸 배치가 바뀌면 같은 종 이웃 여부가 바뀔 수 있음
    void OnTileObjectChanged(TileObjectChangedEvent e)
    {
        if (minionData != null && HeroUpgradeSystem.Get(HeroUpgradeAbility.TwinAttack) > 0f)
            UpdateMinion();
    }

    public virtual void OnDisable()
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
        GameEventBus.Unsubscribe<MinionLevelUpReinforceEvent>(OnMinionLevelUpReinforceEvent);
        GameEventBus.Unsubscribe<TileObjectChangedEvent>(OnTileObjectChanged);
        StopPowerUpEffect();
        ClearBuffEffects();
        StopSpawnDrop(); // 풀 반납 시 몸 위치·크기 원래대로
        ClearEvolution(); // 진화 부품·그림 정리

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
        if (AttackSpeed >= 0 && !IsBeingDragged) // (2026-11-21) 드래그 중엔 공격 멈춤 (타이머도 안 참)
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


    // (2026-11-24) 근접 공격 미니언 (용사 강화 '근거리' 판정. 회전형은 형태로 따로 판정)
    public virtual bool IsMelee => false;

    // 한 번 공격에 발사하는 탄 수 (2 이상이면 서로 다른 타겟을 노림)
    public virtual int ShootCount => 1;

    // 서로 다른 타겟 count개 (우선순위 순). 기본은 findTargetType 기준
    public virtual List<Transform> FindTargets(int count)
    {
        return InGameUtil.FindTargets(transform.position, AttackRange, targetLayerMask, EffectiveFindTargetType, count);
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
        if (shotModifier.tileRune != null)
            shotModifier.tileRune.OnFired(baseBullet);
        if (evolutionAbility != null) // (2026-11-26) 진화 동작 (마그마 스플래시·부채꼴 등)
            evolutionAbility.OnBulletFired(baseBullet);
        TryDoubleShot(baseBullet);
        return true;
    }

    // (2026-11-23) 엘프 종족 버프 칸: 확률(DoubleShotChance %)로 같은 탄을 한 발 더 (연발 룬과 같은 간격, 추가 탄은 OnFired·다시 판정 없음)
    public float DoubleShotChance => statDic.TryGetValue(StatType.DoubleShotChance, out float v) ? v : 0f;

    void TryDoubleShot(AllyBulletObject bullet)
    {
        float chance = DoubleShotChance;
        if (chance <= 0f || Random.Range(0f, 100f) >= chance)
            return;
        DoubleShot(bullet.spec, bullet.allyUnit, bullet.shootPos, bullet.shootDir, bullet.shootDamage).Forget();
    }

    async UniTaskVoid DoubleShot(BulletSpec spec, IAllyUnit allyUnit, Vector2 pos, Vector2 dir, float damage)
    {
        if (await UniTask.Delay(KingSlime.COMBO_ATTACK_INTERVAL_MS, cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow())
            return;
        if (!gameObject.activeInHierarchy)
            return;
        AllyBulletObject extra = spec.Instantiate(allyUnit);
        extra.transform.position = pos;
        extra.Shoot(dir, damage);
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
            if (GameSetting.USE_WAVE_MINION_OFFER)
            {
                pickedMinionKey = key; // (2026-11-19) 카드 소환 모드: 합성 결과 = 같은 종
            }
            else
            {
                UserMinion userMinion = UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions[Random.Range(0, UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions.Length)];
                pickedMinionKey = userMinion.key;
            }
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

    // (2026-11-21) 머지·교체로 새 미니언이 같은 칸에 생길 때, 그 칸에 있던 미니언의 설정(타겟 방식·회전 방향·버프 범위)을 이어받음
    // 새 미니언이 쓸 수 있는 설정만 (종이 달라 그 설정이 없으면 기본값 그대로)
    public void CopyUnitOptionsFrom(Minion src)
    {
        if (src == null || src == this)
            return;
        if (src.UseFindTarget && UseFindTarget)
            SetFindTargetType(src.findTargetType);
        if (src is IRotationDirectionUnit srcRot && this is IRotationDirectionUnit rot)
            rot.SetRotationDirection(srcRot.RotationDirection);
        if (src is IBuffRangeUnit srcBuff && this is IBuffRangeUnit buff)
            buff.SetBuffRangeShape(srcBuff.BuffRangeShape);
    }
    public virtual Transform FindTarget()
    {
        return InGameUtil.FindTarget(transform.position, AttackRange, targetLayerMask, EffectiveFindTargetType);
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
    // (2026-11-21) 드래그 중 = 공격·스킬 쿨타임·스킬 발동 멈춤. 스킬 쿨타임은 SkillDeltaTime이 0을 돌려줘서 멈춤
    public bool IsBeingDragged { get; private set; }
    public virtual void StartDrag()
    {
        IsBeingDragged = true;
    }
    public virtual void StartTarget()
    {
        rangeTr.gameObject.SetActive(true);
    }

    public virtual void EndTarget()
    {
        rangeTr.gameObject.SetActive(false);
    }

    public virtual void EndDrag(Tile t)
    {
        IsBeingDragged = false;
        // rangeTr.gameObject.SetActive(false);
    }
}

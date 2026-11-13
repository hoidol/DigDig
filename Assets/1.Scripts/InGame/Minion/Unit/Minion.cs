using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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
    public virtual void Awake()
    {
        if (rootTr == null)
            rootTr = transform.Find("Root");

    }
    public UserMinion userMinion;
    public MinionEnhanceInfo minionEnhanceInfo;
    public Transform rangeTr;
    public virtual void Spawn(Vector2 pos, int mLv)
    {
        userMinion = UserDataManager.Instance.userMinionManager.GetUserMinion(key);

        minionData = MinionManager.Instance.GetMinionData(key);
        minionEnhanceInfo = minionData.GetCommonMinionEnhanceInfo(userMinion.enhanceLevel);
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
    }

    public virtual void InitMinion()
    {

    }

    public virtual void InitMinionStat()
    {
        statDic[StatType.AttackPower] = minionData.GetMinionStat(StatType.AttackPower, userMinion.enhanceLevel).GetValue<float>(StatMergeLevel);
        statDic[StatType.AttackSpeed] = GetDataStat(StatType.AttackSpeed); // MinionData.csv (강화·머지 레벨 무관)
        statDic[StatType.AttackRange] = GetDataStat(StatType.AttackRange);
        statDic[StatType.CritChance] = GetDataStat(StatType.CritChance); // % 단위. MinionData.csv (기본 0)
    }

    // 데이터 스탯 값 (없으면 0)
    float GetDataStat(StatType statType)
    {
        MinionStat stat = minionData.GetMinionStat(statType, userMinion.enhanceLevel);
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
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Subscribe<MinionLevelUpReinforceEvent>(OnMinionLevelUpReinforceEvent);
    }

    public virtual void OnDisable()
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Unsubscribe<MinionLevelUpReinforceEvent>(OnMinionLevelUpReinforceEvent);
    }

    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        CheckTarget(e.enemy);
    }
    void OnDestroyedStoneEvent(DestroyedStoneEvent e)
    {
        CheckTarget(e.stone);
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
                    shotModifier.CountAttack();
            }
        }

        if (targetTr == null)
        {
            rootTr.localScale = new Vector3(fireDir.x >= 0 ? 1 : -1, 1, 1);
        }
    }


    public Transform targetTr;
    Vector2 fireDir;
    public virtual Vector2 AttackDirecton()
    {
        Vector2 fireDir = Vector2.zero;

        targetTr = FindTarget();
        onTargetListener?.Invoke(targetTr);

        if (targetTr != null)
        {
            fireDir = (targetTr.position - transform.position).normalized;
            rootTr.localScale = new Vector3(fireDir.x >= 0 ? 1 : -1, 1, 1);
        }

        return fireDir.normalized;
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
            // 타겟이 발사 수보다 적으면 앞 타겟부터 다시 순환
            for (int i = 0; i < count; i++)
            {
                Transform target = targets[i % targets.Count];
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
    }

    public void RemoveBuff(Buff buff)
    {
        activeBuffs.Remove(buff);
        UpdateMinion();
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

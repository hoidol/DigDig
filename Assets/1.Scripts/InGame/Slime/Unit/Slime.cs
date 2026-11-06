using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public abstract class Slime : MonoBehaviour, IAllyUnit, ITileObject, IBuffable, IShotModifiable
{
    public string key;
    public int mergeLevel;

    // 진화 (EvolutionTileItem: 최대 머지 레벨 슬라임이 올라가면 진화). 진화 내용은 추후 기획 → 지금은 표시만
    public bool isEvolved;
    public virtual bool CanEvolve => mergeLevel >= SlimeData.MAX_MERGE_LEVEL && !isEvolved;
    public virtual void Evolve()
    {
        if (!CanEvolve)
            return;
        isEvolved = true;
        Debug.Log($"[Evolve] {key} 진화 (머지 레벨 {mergeLevel})");
        // TODO: 진화 효과 (기획 확정 후 슬라임별 재정의 or 공용 처리)
    }

    public SlimeEnhanceAbility[] slimeEnhanceAbilities;
    // public SlimeMovement movement;

    public Dictionary<StatType, float> statDic = new Dictionary<StatType, float>();

    // attackPowerRandomRatio > 0이면 매 공격마다 중간값 ±비율 범위 랜덤
    public virtual float AttackPower => SlimeData.ApplyRandomRatio(statDic[StatType.AttackPower], slimeData.attackPowerRandomRatio);
    public float AttackSpeed => statDic[StatType.AttackSpeed];
    public float AttackRange => statDic[StatType.AttackRange];
    public float CritChance => statDic[StatType.CritChance]; // % 단위. 기본값은 SlimeData.csv CritChance, 버프(설의 등)로 오름

    public Transform rootTr;
    public SlimeData SlimeData => SlimeManager.Instance.GetSlimeData(key);
    public float AccumulatedDamage { get; set; }

    public float attackTimer;
    //적 찾는 방식 설정
    public FindTargetType findTargetType = FindTargetType.Closest;

    public FindTargetType FindTargetType => findTargetType;

    public void AccumulateDamage(float d)
    {
        AccumulatedDamage += d;
    }
    public Transform Transform => transform;
    public string Key => key;
    public AllyType AllyType => AllyType.Slime;
    public AttackType AttackType => slimeData.attackType;
    public LayerMask targetLayerMask = 1 << 3;


    public Action<Transform> onTargetListener;
    public SlimeData slimeData;
    public List<Buff> activeBuffs = new List<Buff>();
    public ShotModifier shotModifier = new ShotModifier(); // 타일 아이템의 발사 방식 효과
    public ShotModifier ShotModifier => shotModifier;
    public virtual void Awake()
    {
        slimeEnhanceAbilities = GetComponentsInChildren<SlimeEnhanceAbility>();
        
        if (rootTr == null)
            rootTr = transform.Find("Root");

    }
    public UserSlime userSlime;
    public SlimeEnhanceInfo slimeEnhanceInfo;
    public Transform rangeTr;
    public virtual void Spawn(Vector2 pos, int mLv)
    {
        userSlime = UserDataManager.Instance.userSlimeManager.GetUserSlime(key);

        slimeData = SlimeManager.Instance.GetSlimeData(key);
        slimeEnhanceInfo = slimeData.GetCommonSlimeEnhanceInfo(userSlime.enhanceLevel);
        transform.position = pos;
        this.mergeLevel = mLv;
        isDraging = false;
        isEvolved = false; // 풀링 재사용 · 합성/교체로 새로 생긴 슬라임은 진화 안 된 상태

        statDic.Clear();
        statDic.Add(StatType.AttackPower, 0);
        statDic.Add(StatType.AttackSpeed, 0);
        statDic.Add(StatType.AttackRange, 0);
        statDic.Add(StatType.CritChance, 0);
        rangeTr.gameObject.SetActive(false);

        InitSlime();
        UpdateSlime();
    }

    public virtual void InitSlime()
    {

    }

    public virtual void InitSlimeStat()
    {
        statDic[StatType.AttackPower] = slimeData.GetSlimeStat(StatType.AttackPower, userSlime.enhanceLevel).GetValue<float>(mergeLevel);
        statDic[StatType.AttackSpeed] = GetDataStat(StatType.AttackSpeed); // SlimeData.csv (강화·머지 레벨 무관)
        statDic[StatType.AttackRange] = GetDataStat(StatType.AttackRange);
        statDic[StatType.CritChance] = GetDataStat(StatType.CritChance); // % 단위. SlimeData.csv (기본 0)
    }

    // 데이터 스탯 값 (없으면 0)
    float GetDataStat(StatType statType)
    {
        SlimeStat stat = slimeData.GetSlimeStat(statType, userSlime.enhanceLevel);
        return stat != null ? stat.GetValue<float>(mergeLevel) : 0f;
    }

    public virtual void UpdateSlime()
    {
        InitSlimeStat();
        UpdateReinforce();
        
        foreach (var buff in activeBuffs)
        {
            statDic.TryGetValue(buff.statType, out float cur); // 없는 스탯 버프도 예외 없이 (기본 0)
            statDic[buff.statType] = buff.Apply(cur);
        }

        // 사거리 버프까지 반영된 값으로 표시
        rangeTr.localScale = Vector3.one * statDic[StatType.AttackRange];
    }

    public SlimeEnhanceAbility GetSlimeEnhanceAbility(int lv)
    {
        for (int i = 0; i < slimeEnhanceAbilities.Length; i++)
        {
            if (slimeEnhanceAbilities[i].level == lv)
            {
                return slimeEnhanceAbilities[i];
            }
        }
        return null;
    }
    public virtual void OnEnable()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Subscribe<SlimeLevelUpReinforceEvent>(OnSlimeLevelUpReinforceEvent);
    }

    public virtual void OnDisable()
    {
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Unsubscribe<SlimeLevelUpReinforceEvent>(OnSlimeLevelUpReinforceEvent);
    }

    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        CheckTarget(e.enemy);
    }
    void OnDestroyedStoneEvent(DestroyedStoneEvent e)
    {
        CheckTarget(e.stone);
    }

    void OnSlimeLevelUpReinforceEvent(SlimeLevelUpReinforceEvent e)
    {
        if (slimeData.key == e.key)
        {
            UpdateSlime();            
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
        int reinforceLevel = KingSlime.Instance.statMgr.GetReinforceLevel(slimeData.key);
        statDic[StatType.AttackPower] *= 1f + slimeData.reinforceAttackPowerPercent / 100f * reinforceLevel;
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

    public virtual async UniTask<(bool, string, int)> Merge(Slime target)
    {
        if (SlimeData.growth > 1 || target.SlimeData.growth > 1)
            return (false, null, 0);
        if (SlimeData.growth != target.SlimeData.growth)
            return (false, null, 0);
        if (mergeLevel != target.mergeLevel)
            return (false, null, 0);
        if (target.key != key)
            return (false, null, 0);
        if (mergeLevel >= SlimeData.MAX_MERGE_LEVEL || target.mergeLevel >= SlimeData.MAX_MERGE_LEVEL)
            return (false, null, 0);

        string pickedSlimeKey = null;
        int lv = 0;
        if (SlimeData.growth == 1)
        {
            UserSlime userSlime = UserDataManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes[Random.Range(0, UserDataManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes.Length)];
            pickedSlimeKey = userSlime.key;
            lv = mergeLevel + 1;
        }
        else if (SlimeData.growth == 0)
        {
            UserSlime userSlime = UserDataManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes[Random.Range(0, UserDataManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes.Length)];
            pickedSlimeKey = userSlime.key;
        }

        return (true, pickedSlimeKey, lv);
    }
    public void SetFindTargetType(FindTargetType type)
    {
        findTargetType = type;
    }
    public virtual Transform FindTarget()
    {
        return InGameUtil.FindTarget(transform.position, AttackRange, targetLayerMask, findTargetType);
    }


    public void AddBuff(Buff buff)
    {
        activeBuffs.Add(buff);
        UpdateSlime();
    }

    public void RemoveBuff(Buff buff)
    {
        activeBuffs.Remove(buff);
        UpdateSlime();
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

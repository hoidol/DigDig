using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using TMPro;
public abstract class Enemy : MonoBehaviour, IHittable
{
    public EnemyType enemyType; // 적 종류 구분

    public DefenceType defenceType;
    public EnemyData enemyData; //게임 데이터
    [field: SerializeField]
    public StatusEffectHandler statusEffectHandler
    {

        get; private set;
    }
    public float MaxHp => maxHp;
    [SerializeField] public float maxHp;//{ get; private set; }
    public float CurHp => curHp;
    [field: SerializeField] public float curHp;// { get; private set; }

    [SerializeField] protected Transform root;
    [SerializeField] protected Transform hpPoint;
    public Transform HpPoint => hpPoint != null ? hpPoint : transform; // 머리 위 체력바·데미지 텍스트 위치
    // 머리 위 체력바(EnemyWorldCanvas) 사용 여부. 보스는 BossCanvas로 표시
    public virtual bool UseHpUI => true;
    protected Rigidbody2D rg2d;
    public Rigidbody2D Rigidbody2D => rg2d;
    protected Collider2D col2d;
    public bool followingPlayer;

    public Transform Transform => transform;


    public float apearTime = 2; //떨어지면서 등장하는 시간
    public const float MOVE_SPEED = 2; //떨어지면서 등장하는 시간

    public EnemyDamageData damageData = new EnemyDamageData();
    // public TMP_Text hpText;
    public int pathIdx; // 경로 모드에서 다음에 향할 EnemyFieldController.paths 인덱스
    Vector2 moveDir; // pathIdx 지점으로 전환할 때 캐싱한 이동 방향 (목표 지점을 지나쳤는지 판정용)
    int moveDirIdx = -1; // moveDir가 어느 pathIdx 기준인지. -1이면 다시 계산
    [SerializeField] private  EnemyDefence enemyDefence;

    // 스프라이트 프리팹(자식)의 Animator. Walk = 기본(이동), 죽으면 Dead 재생 후 끝나면 비활성화
    public Animator animator;
    public static readonly int WALK_ANIM_HASH = Animator.StringToHash("Walk");
    public static readonly int DEAD_ANIM_HASH = Animator.StringToHash("Dead");
    // Dead 애니메이션이 끝나지 않는 경우(상태 없음·속도 0 등) 대비 최대 대기 시간
    public const float DEAD_ANIM_MAX_TIME = 3f;
    // Dead 애니메이션이 완전히 끝나기 이만큼(초) 전에 비활성화 (마지막 프레임에 멈춰 보이거나 루프 클립이 처음으로 튀는 것 방지). 최소 1프레임 전
    public const float DEAD_ANIM_END_EARLY_TIME = 0.1f;
    // 처치되어 Dead 애니메이션 재생 중 (충돌·이동 없음, 끝나면 비활성화 후 풀로)
    public bool IsDying { get; private set; }
    float dyingTimer;

    public virtual void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);
        rg2d = GetComponent<Rigidbody2D>();
        col2d = GetComponent<Collider2D>();
        statusEffectHandler = GetComponent<StatusEffectHandler>();
        statusEffectHandler?.Init();
        enemyDefence = GetComponent<EnemyDefence>();
        // 프리팹에 방어 컴포넌트가 없으면 defenceType에 맞는 것을 붙임
        if (enemyDefence == null)
            enemyDefence = (EnemyDefence)gameObject.AddComponent(EnemyDefence.GetDefenceClass(DefenceType));
    }
    //적 생성 시 호출
    public int wave;
    public AllyUnitDamageData killedBy; // 처치한 공격 (코인 타일이 처치한 유닛인지 확인. 지급 후 null)
    // 이동 막기 (철벽 슬라임 방패 등). 막는 쪽이 여러 개일 수 있어 개수로 관리, 0이면 이동
    int blockerCount;
    public bool IsBlocked => CanBeBlocked && blockerCount > 0;
    // 엘리트/보스는 막기 무시 (방패에 닿아도 계속 이동, 막기 슬롯도 차지하지 않음)
    public virtual bool CanBeBlocked => !IsEliteOrBoss;
    public bool IsEliteOrBoss => this is EliteEnemy || this is Boss;
    public void AddBlocker() => blockerCount++;
    public void RemoveBlocker() => blockerCount = Mathf.Max(0, blockerCount - 1);
    // 풀링 재사용 구분용 (Spawn마다 증가). 막는 쪽이 이전 생애의 적을 해제하지 않도록 확인
    public int SpawnId { get; private set; }

    public virtual void Spawn(Vector2 pos, int wave)
    {
        this.wave = wave;
        killedBy = null;
        IsDying = false;
        // 위치를 먼저 정하고 활성화 → 풀에서 꺼낸 적이 이전 위치(죽은 자리)에서 켜졌다가 옮겨지지 않게.
        // rigidbody 위치도 바로 맞춤 (transform만 바꾸면 다음 물리 단계까지 이전 위치에 남아 한두 프레임 튐)
        transform.position = pos;
        gameObject.SetActive(true);
        rg2d.position = pos;
        rg2d.linearVelocity = Vector2.zero;
        statusEffectHandler?.Reset();
        pathIdx = 0;
        moveDirIdx = -1;
        blockerCount = 0;
        SpawnId++;
        Apear();
        maxHp = GetHp();
        curHp = maxHp;
        if (UseHpUI)
            EnemyWorldCanvas.Instance?.Attach(this); // 머리 위 체력바
        // 풀에서 재사용될 때 Dead 상태에 멈춰 있지 않도록 Walk부터
        PlayAnim(WALK_ANIM_HASH);

        // damageData.damage = GetAttackPower();
    }

    // 최종 체력 = 페이즈 체력 * 에너미 배율
    public float GetHp()
    {
        float hp = GetWaveHp(wave);

        // 시간에 따른 체력 증가 (주석 처리)
        // float nextHp = GetWaveHp(wave + 1);
        // // 10초 단위로 끊어서 웨이브 진행률 계산 (0~1)
        // float elapsed = GameSetting.WAVE_TIME - GameManager.Instance.mainWaveTimer;
        // int timeStep = Mathf.FloorToInt(elapsed / GameSetting.ENEMY_HP_INCREASE_INTERVAL);
        // float progress = Mathf.Clamp01(timeStep * GameSetting.ENEMY_HP_INCREASE_INTERVAL / GameSetting.WAVE_TIME);
        // hp += (nextHp - hp) * GameSetting.ENEMY_HP_REACH_RATIO * progress;

        return hp * enemyData.hpMultiplier;
    }

    // 페이즈 시작 체력 = enemyHp * (1 + addHpRatioPerWave * wave) * multiHpPerWave^wave
    float GetWaveHp(int wave)
    {

        StageData stageData = GameManager.Instance.stageData;

        // float hp = stageData.enemyHp + (stageData.enemyHp * (1f + stageData.addHpRatioPerWave) * (float)wave);
        // float hp = stageData.enemyHp + stageData.enemyHp * ((1 + stageData.addHpRatioPerWave) * wave);
        float hp = stageData.enemyHp * (1 + stageData.addHpRatioPerWave * wave);

        return hp * Mathf.Pow(stageData.multiHpPerWave, wave);
    }

    // 최종 공격력 = enemyAtk * (1 + addAtkRatioPerWave * wave) * multiAtkPerWave^wave * 에너미 배율
    // public float GetAttackPower()
    // {
    //     StageData stageData = GameManager.Instance.stageData;
    //     int wave = GameManager.Instance.wave;
    //     float atk = stageData.enemyAtk * (1 + stageData.addAtkRatioPerWave * wave);
    //     atk *= Mathf.Pow(stageData.multiAtkPerWave, wave);
    //     return atk * enemyData.attackPowerMultiplier;
    // }

    const float APEAR_POP_DURATION = 0.15f;
    public virtual void Apear()
    {
        gameObject.SetActive(true);

        if (col2d != null)
            col2d.enabled = false;

        transform.DOKill();
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, APEAR_POP_DURATION).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // 등장 중에 죽었으면 충돌을 다시 켜지 않음
                if (col2d != null && !IsDying)
                    col2d.enabled = true;
            });
    }


    // 모든 적 정지 (모래시계 권능). Time.time 기준
    static float freezeAllUntil;
    public static bool IsFrozenAll => Time.time < freezeAllUntil;
    public static void FreezeAll(float seconds) => freezeAllUntil = Mathf.Max(freezeAllUntil, Time.time + seconds);
    public static void ClearFreezeAll() => freezeAllUntil = 0f;

    public virtual void Update()
    {
        if (IsDying)
        {
            UpdateDying();
            return;
        }

        if (!GameManager.Instance.isPlaying)
        {
            rg2d.linearVelocity = Vector2.zero;
            return;
        }

        if ((statusEffectHandler != null && statusEffectHandler.IsStunned) || IsBlocked || IsFrozenAll)
        {
            rg2d.linearVelocity = Vector2.zero;
            return;
        }


        UpdateMoving();
    }

    const float PATH_ARRIVE_DISTANCE = 0.1f;

    // paths[pathIdx]를 향해 이동, 도착하면 다음 지점으로. 마지막 지점 도착 시 ReachPathEnd
    // 방향은 지점 전환 시 한 번만 계산(moveDir)하고, 남은 거리를 그 방향으로 투영해서
    // 0 이하(= 지나침)거나 이번 프레임 이동량 이하면 지점에 스냅 후 다음 지점으로 → 목표를 넘어가지 않음
    public virtual void UpdateMoving()
    {
        Transform[] paths = EnemyFieldController.Instance.paths;
        float speed = enemyData.moveSpeed * (statusEffectHandler != null ? statusEffectHandler.SlowRate : 1f);
        float arriveDist = Mathf.Max(PATH_ARRIVE_DISTANCE, speed * Time.deltaTime);

        Vector2 pos = transform.position;
        while (pathIdx < paths.Length)
        {
            Vector2 target = paths[pathIdx].position;
            if (moveDirIdx != pathIdx)
            {
                moveDir = (target - pos).normalized;
                moveDirIdx = pathIdx;
            }

            if (Vector2.Dot(target - pos, moveDir) > arriveDist)
                break;

            pos = target;
            transform.position = target;
            pathIdx++;
        }

        if (pathIdx >= paths.Length)
        {
            ReachPathEnd();
            return;
        }

        rg2d.linearVelocity = moveDir * speed;
        UpdateFacing(moveDir);
    }

    // 경로 끝 도착: 캐릭터 체력 감소 후 제거 (보상 없음)
    protected virtual void ReachPathEnd()
    {
        rg2d.linearVelocity = Vector2.zero;
        KingSlime.Instance.TakeDamage(damageData);
        Destroy();
    }


    public virtual void ApplyStatusEffect(StatusEffect effect)
    {
        if (statusEffectHandler == null)
        {
            statusEffectHandler = GetComponent<StatusEffectHandler>();
            if (statusEffectHandler == null)
            {
                statusEffectHandler = gameObject.AddComponent<StatusEffectHandler>();
            }
        }
        statusEffectHandler.Apply(effect);
    }
    public virtual void CancelAttack()
    {

    }



    // IHittable 인터페이스 구현 부
    public virtual void TakeDamage(DamageData damage)
    {
        if (curHp <= 0)
            return;
        // 방어 타입 배율, 만능 공격, 거인 사냥 배율은 EnemyDefence에서 계산
        float multiplier = enemyDefence.CalculateDamage(damage);
        // 낙인 중첩만큼 받는 피해 증가 (모든 공격)
        if (statusEffectHandler != null)
            multiplier *= statusEffectHandler.StigmaDamageMul;
        curHp = Mathf.Max(0, curHp - damage.ApplyDamage(hpPoint.transform.position, multiplier));
        // if (curHp > 1)
        //     hpText.text = ((int)curHp).ToString();
        // else if (0 < curHp && curHp <= 1)
        //     hpText.text = "1";

        OnHpChanged();
        if (curHp < 1)
        {
            curHp = 0; // 0~1 사이로 남으면 같은 프레임 다른 탄에 다시 죽어 EnemyDeadEvent가 중복 발행됨
            killedBy = damage as AllyUnitDamageData;
            Reward();
            Destroy();
        }
    }

    // 프리팹에 지정된 방어 타입 (StageData 적 프리팹 딕셔너리 키와 같은 값). 엘리트는 EnemyData를 Normal로 공유하므로 enemyData 기준이 아님
    public DefenceType DefenceType => defenceType;

    public int face;
    public void UpdateFacing(Vector2 vec)
    {
        face = vec.x >= 0 ? 1 : -1;
        root.localScale = new Vector3(face, 1, 1);
    }


    protected virtual void OnHpChanged()
    {
        EnemyWorldCanvas.Instance?.Refresh(this);
    }
    public virtual void Reward()
    {
        
    }
    // 처치(체력 0) 또는 경로 끝 도착 시. 처치면 Dead 애니메이션 후 비활성화, 경로 끝이면 바로 비활성화
    public virtual void Destroy()
    {
        if (IsDying)
            return;
        EffectManager.Instance.Play(EffectType.StoneBreak, transform.position);
        GameEventBus.Publish(new EnemyDeadEvent(this));

        if (curHp <= 0 && HasAnimState(DEAD_ANIM_HASH))
            StartDying();
        else
            Deactivate();
    }

    void StartDying()
    {
        IsDying = true;
        dyingTimer = 0;
        if (col2d != null)
            col2d.enabled = false;
        rg2d.linearVelocity = Vector2.zero;
        EnemyWorldCanvas.Instance?.Release(this); // Dead 애니메이션 중엔 체력바 숨김
        PlayAnim(DEAD_ANIM_HASH);
    }

    // Dead 상태의 재생이 한 번 끝나기 직전(DEAD_ANIM_END_EARLY_TIME 전)에 비활성화. Dead 클립이 루프여도 한 번만 보여줌
    void UpdateDying()
    {
        rg2d.linearVelocity = Vector2.zero;
        dyingTimer += Time.deltaTime;
        if (dyingTimer >= DEAD_ANIM_MAX_TIME || IsDeadAnimEnd())
            Deactivate();
    }

    bool IsDeadAnimEnd()
    {
        if (animator == null || !animator.isActiveAndEnabled)
            return true;
        if (animator.IsInTransition(0))
            return false;
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.shortNameHash != DEAD_ANIM_HASH)
            return false;
        // 남은 재생 시간(초) = 남은 비율 × 클립 길이 / 재생 속도
        float speed = Mathf.Abs(info.speed * info.speedMultiplier * animator.speed);
        if (speed <= 0f)
            return false; // 멈춰 있으면 DEAD_ANIM_MAX_TIME으로 종료
        float remaining = (1f - info.normalizedTime) * info.length / speed;
        return remaining <= Mathf.Max(DEAD_ANIM_END_EARLY_TIME, Time.deltaTime);
    }

    // 비활성화 후 스포너 풀로 반환 (스포너가 만든 적이 아니면 비활성화만)
    public void Deactivate()
    {
        IsDying = false;
        EnemyWorldCanvas.Instance?.Release(this);
        gameObject.SetActive(false);
        if (EnemyFieldController.Instance != null)
            EnemyFieldController.Instance.ReturnToPool(this);
    }

    bool HasAnimState(int hash) => animator != null && animator.runtimeAnimatorController != null && animator.HasState(0, hash);

    void PlayAnim(int hash)
    {
        if (HasAnimState(hash))
            animator.Play(hash, 0, 0f);
    }

    public bool CanHit()
    {
        return curHp > 0 && !IsDying;
    }

}

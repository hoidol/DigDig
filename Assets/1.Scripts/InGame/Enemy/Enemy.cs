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
    protected Rigidbody2D rg2d;
    public Rigidbody2D Rigidbody2D => rg2d;
    protected Collider2D col2d;
    public bool followingPlayer;

    public Transform Transform => transform;


    public float apearTime = 2; //떨어지면서 등장하는 시간
    public const float MOVE_SPEED = 2; //떨어지면서 등장하는 시간

    public EnemyDamageData damageData = new EnemyDamageData();
    public TMP_Text hpText;
    public int pathIdx; // 경로 모드에서 다음에 향할 EnemySpawner.paths 인덱스
    Vector2 moveDir; // pathIdx 지점으로 전환할 때 캐싱한 이동 방향 (목표 지점을 지나쳤는지 판정용)
    int moveDirIdx = -1; // moveDir가 어느 pathIdx 기준인지. -1이면 다시 계산
    public virtual void Awake()
    {
        rg2d = GetComponent<Rigidbody2D>();
        col2d = GetComponent<Collider2D>();
        statusEffectHandler = GetComponent<StatusEffectHandler>();
        statusEffectHandler?.Init();
    }
    //적 생성 시 호출
    public int wave;
    public AllyUnitDamageData killedBy; // 처치한 공격 (코인 타일이 처치한 유닛인지 확인. 지급 후 null)
    public virtual void Spawn(Vector2 pos, int wave)
    {
        this.wave = wave;
        killedBy = null;
        gameObject.SetActive(gameObject);
        statusEffectHandler?.Reset();
        pathIdx = 0;
        moveDirIdx = -1;
        Apear();
        transform.position = pos;
        maxHp = GetHp();
        curHp = maxHp;
        hpText.text = ((int)curHp).ToString();

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

        transform.localScale = Vector3.zero;
        transform.DOScale(1f, APEAR_POP_DURATION).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                if (col2d != null)
                    col2d.enabled = true;
            });
    }


    public virtual void Update()
    {
        if (!GameManager.Instance.isPlaying)
        {
            rg2d.linearVelocity = Vector2.zero;
            return;
        }

        if (isPushing)
        {
            if (rg2d.linearVelocity.magnitude < 0.1f)
            {
                isPushing = false;
                moveDirIdx = -1; // 밀려서 경로를 벗어났으므로 현재 위치 기준으로 방향 다시 계산
            }
        }

        if (statusEffectHandler != null && statusEffectHandler.IsStunned)
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
        if (isPushing)
            return;

        Transform[] paths = EnemySpawner.Instance.paths;
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
        Character.Instance.TakeDamage(damageData);
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

        // 만능 공격(AllRounder 속성 / 만능 공격 타일): 방어 타입 배율 무시 (모든 방어 타입에 100%)
        float multiplier = damage.IgnoreDefence ? 1f : GetDamageMultiplier(damage.attackType);
        // 거인 사냥 타일: 엘리트/보스 추가 배율
        if (damage is AllyUnitDamageData ad && (this is EliteEnemy || this is Boss))
            multiplier *= ad.eliteBossDamageMul;
        curHp = Mathf.Max(0, curHp - damage.ApplyDamage(hpPoint.transform.position, multiplier));
        if (curHp > 1)
            hpText.text = ((int)curHp).ToString();
        else if (0 < curHp && curHp <= 1)
            hpText.text = "1";

        OnHpChanged();
        if (curHp < 1)
        {
            curHp = 0; // 0~1 사이로 남으면 같은 프레임 다른 탄에 다시 죽어 EnemyDeadEvent가 중복 발행됨
            killedBy = damage as AllyUnitDamageData;
            Reward();
            Destroy();
        }
    }

    public DefenceType DefenceType => enemyData != null ? enemyData.defenceType : DefenceType.Normal;

    // 공격 속성 × 방어 타입 배율 (DefenceTable). 방어형별 특수 처리가 필요하면 override
    protected virtual float GetDamageMultiplier(AttackType attackType)
    {
        return DefenceInfo.Get(attackType, DefenceType);
    }

    public int face;
    public void UpdateFacing(Vector2 vec)
    {
        face = vec.x >= 0 ? 1 : -1;
        root.localScale = new Vector3(face, 1, 1);
    }


    protected virtual void OnHpChanged()
    {

    }
    public virtual void Reward()
    {
        //Exp.Instantiate(transform.position, enemyData.exp, 1);
        EffectManager.Instance.Play(EffectType.StoneBreak, transform.position);
    }
    public virtual void Destroy()
    {
        GameEventBus.Publish(new EnemyDeadEvent(this));
    }
    public bool CanHit()
    {
        return curHp > 0;
    }

    public bool isPushing;
    public void Push(Vector2 dir, float power)
    {
        isPushing = true;
        rg2d.AddForce(dir * power, ForceMode2D.Impulse);
    }

}

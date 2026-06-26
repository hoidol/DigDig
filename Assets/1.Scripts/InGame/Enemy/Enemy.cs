using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using DG.Tweening;
using Cysharp.Threading.Tasks;
public abstract class Enemy : MonoBehaviour, IHittable, ITile
{
    public EnemyType enemyType; // 적 종류 구분
    public EnemyState state { get; private set; } // 적 상태 - FSM 패턴
    public EnemyData enemyData { get; private set; } //게임 데이터
    [field: SerializeField]
    public StatusEffectHandler statusEffectHandler
    {

        get; private set;
    }
    [Header("생성 시 타일을 부수면서 등장함")]
    public bool breakTileWhenSpawn;
    #region 
    [SerializeField] public float maxHp;//{ get; private set; }
    [field: SerializeField] public float curHp;// { get; private set; }

    [SerializeField] Transform root;
    [SerializeField] protected Transform hpPoint;
    protected Rigidbody2D rg2d;
    public Rigidbody2D Rigidbody2D => rg2d;
    protected float attackTimer;
    protected bool attacking;

    bool isPushed;
    Coroutine pushCoroutine;

    public Transform Transform => transform;

    public Vector2Int[,] TileIndexArr => tileIndexArr;
    public Vector2Int Size => enemyData.size;

    public bool BreakTileWhenSpawn => breakTileWhenSpawn;
    public Vector2Int[,] tileIndexArr;

#if UNITY_EDITOR
    List<Vector2Int> tileIndexList = new List<Vector2Int>();
#endif
    public float apearTime = 2; //떨어지면서 등장하는 시간
    public const float MOVE_SPEED = 2; //떨어지면서 등장하는 시간


    protected bool moving;
    protected float moveTimer;
    public Animator animator;
    #endregion

    protected virtual void Awake()
    {
        rg2d = GetComponent<Rigidbody2D>();
        statusEffectHandler = GetComponent<StatusEffectHandler>();
        animator = GetComponentInChildren<Animator>();
    }
    //Enemy 게임 데이터 설정
    public virtual void Init(EnemyData data)
    {
        enemyData = data;
        statusEffectHandler.Init();
    }
    //적 생성 시 호출
    public virtual void Spawn(Vector2Int[,] idxArr)
    {
        moveTimer = MOVE_SPEED;
        //apearTime 초 후에 등장
        moving = false;
        tileIndexArr = new Vector2Int[enemyData.size.x, enemyData.size.y];

#if UNITY_EDITOR
        foreach (var tileIndex in idxArr)
        {
            tileIndexList.Add(tileIndex);
        }

#endif
        RegisterTile(idxArr);

        gameObject.SetActive(false);
        Vector2 pos = MapManager.TileIndexToCenterPosition(idxArr);

        EnemySpawnIndicator.Get(pos, null).PlayIndicator(tileIndexArr, apearTime, () =>
        {
            Apear();
        });

        transform.position = pos;
        maxHp = enemyData.GetHp();
        curHp = maxHp;
        ChangeState(EnemyState.Waiting);

        attackTimer = 0;
        attacking = false;
    }
    public virtual void Apear()
    {
        gameObject.SetActive(true);
    }

    //상태 전환
    public void ChangeState(EnemyState state)
    {
        this.state = state;
    }


    public virtual void Update()
    {
        if (statusEffectHandler.IsStunned)
        {
            if (attacking)
            {
                CancelAttack();
            }
            return;
        }

        if (moveTimer > 0)
            moveTimer -= Time.deltaTime;

        if (isPushed)
            return;

        if (attackTimer < enemyData.attackSpeed)
            attackTimer += Time.deltaTime;

        if (attacking)
            return;

        if (state == EnemyState.Waiting) UpdateWaiting();
        else if (state == EnemyState.Moving) UpdateMoving();
        else if (state == EnemyState.Attack) UpdateAttack();

    }

    public void Push(Vector2 force)
    {
        if (state == EnemyState.Dead) return;
        if (pushCoroutine != null) StopCoroutine(pushCoroutine);
        pushCoroutine = StartCoroutine(PushRoutine(force));
    }

    IEnumerator PushRoutine(Vector2 force)
    {
        isPushed = true;
        rg2d.AddForce(force, ForceMode2D.Impulse);
        yield return new WaitUntil(() => rg2d.linearVelocity.sqrMagnitude < 0.01f);
        isPushed = false;
        pushCoroutine = null;
    }

    public void ApplyStatusEffect(StatusEffect effect)
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

    //상태가 Waiting 인 경우 처리
    public virtual void UpdateWaiting()
    {
        Vector2 vec = Player.Instance.transform.position - transform.position;

        if (vec.magnitude > enemyData.moveRange && moveTimer <= 0)
        {
            ChangeState(EnemyState.Moving);
            return;
        }
        if (vec.magnitude <= enemyData.attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }
        SetFacing(vec.x);

    }
    public virtual void UpdateMoving()
    {
        if (moving)
            return;
        // Debug.Log("ENemy 이동 시도");
        Vector2 vec = Player.Instance.transform.position - transform.position;
        Vector2Int[] dirs = new Vector2Int[2];
        if (Mathf.Abs(vec.normalized.x) > Mathf.Abs(vec.normalized.y))
        {
            dirs[0] = vec.normalized.x > 0 ? Vector2Int.right : Vector2Int.left;
            dirs[1] = vec.normalized.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
        else
        {
            dirs[0] = vec.normalized.y > 0 ? Vector2Int.up : Vector2Int.down;
            dirs[1] = vec.normalized.x > 0 ? Vector2Int.right : Vector2Int.left;
        }

        for (int i = 0; i < dirs.Length; i++)
        {
            // Debug.Log($"현 위치 {tileIndexArr[0, 0]} 방향 {dirs[i]}");
            if (!MapManager.CheckMoveTo(tileIndexArr, dirs[i]))
                continue;

            MoveTo(dirs[i]).Forget();
            return;
        }

        //이동 못하는 경우
        ChangeState(EnemyState.Waiting);
        moveTimer = MOVE_SPEED;
    }

    public async virtual UniTaskVoid MoveTo(Vector2Int dir)
    {
        if (moving) return;

        moving = true;
        Vector2Int[,] newTiles = MapManager.GetIndexArray(tileIndexArr, dir);
        MapManager.RegisterTile(newTiles,this); // 이동 중 다른 오브젝트가 점유 못하게 선점

        Vector2 dest = MapManager.TileIndexToCenterPosition(newTiles);
        SetFacing(dir.x);
        // await UniTask.Delay(3000);
        transform.DOMove(dest, 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                MapManager.ReleaseTile(tileIndexArr);//현재 위치 해제
                RegisterTile(newTiles); //이동한 위치 등록
            });

        await UniTask.Delay(500);

        moving = false;
        moveTimer = MOVE_SPEED;
        ChangeState(EnemyState.Waiting);
    }

    //상태가 Attack 인 경우 처리
    public abstract void UpdateAttack();


    protected virtual void StartAttack()
    {
        attackTimer = 0;
        attacking = true;
    }

    protected void EndAttack()
    {
        attacking = false;
        ChangeState(EnemyState.Waiting);
    }

    // IHittable 인터페이스 구현 부
    public virtual void TakeDamage(DamageData damage)
    {
        if (state == EnemyState.Dead)
            return;
        damage.Applyed(hpPoint.transform.position);
        curHp = Mathf.Max(0, curHp - damage.damage);

        //Hit Effect
        

        OnHpChanged();
        if (curHp <= 0)
        {
            Reward();
            OnDead();
        }
    }
    public int face;
    public void SetFacing(float dirX)
    {
        face = dirX >= 0 ? 1 : -1;
        root.localScale = new Vector3(face, 1, 1);
    }

    protected virtual void OnHpChanged()
    {

    }
    public void Reward()
    {
        ExpText.SetText((Vector2)hpPoint.position + UnityEngine.Random.insideUnitCircle * 0.3f, "1");
        Player.Instance.AddExp(1);
    }
    public virtual void OnDead()
    {
        ReleaseTile();
        ChangeState(EnemyState.Dead);
    }
    public bool CanHit()
    {
        return curHp > 0;
    }

    public void RegisterTile(Vector2Int[,] idxArr)
    {
        tileIndexArr = idxArr;
        MapManager.RegisterTile(idxArr,this);

    }

    public void ReleaseTile()
    {
        transform.DOKill();
        MapManager.ReleaseTile(tileIndexArr);
        gameObject.SetActive(false);
        GameEventBus.Publish(new EnemyDeadEvent(this));
    }


}

public enum EnemyState
{
    Waiting,
    Moving,
    Attack,
    PhaseTransition,
    Dash,
    Dead
}
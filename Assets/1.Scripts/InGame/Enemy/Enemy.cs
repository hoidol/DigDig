using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using TMPro;
public abstract class Enemy : MonoBehaviour, IHittable, ITile
{
    public EnemyType enemyType; // 적 종류 구분
    public EnemyData enemyData; //게임 데이터
    [field: SerializeField]
    public StatusEffectHandler statusEffectHandler
    {

        get; private set;
    }
    [Header("생성 시 타일을 부수면서 등장함")]
    public bool breakTileWhenSpawn;
    public float MaxHp => maxHp;
    [SerializeField] public float maxHp;//{ get; private set; }
    public float CurHp => curHp;
    [field: SerializeField] public float curHp;// { get; private set; }

    [SerializeField] Transform root;
    [SerializeField] protected Transform hpPoint;
    protected Rigidbody2D rg2d;
    public Rigidbody2D Rigidbody2D => rg2d;
    protected Collider2D col2d;


    public Transform Transform => transform;

    public Vector2Int[,] TileIndexArr => tileIndexArr;
    protected Vector2Int[,] tileIndexArr; //현재 차지하고 있는 위치
    public Vector2Int Size => enemyData.size;

    public bool BreakTileWhenSpawn => breakTileWhenSpawn;

    public float apearTime = 2; //떨어지면서 등장하는 시간
    public const float MOVE_SPEED = 2; //떨어지면서 등장하는 시간

    public DamageData damageData = new DamageData();
    public TMP_Text hpText;

    public virtual void Awake()
    {
        rg2d = GetComponent<Rigidbody2D>();
        col2d = GetComponent<Collider2D>();
        statusEffectHandler = GetComponent<StatusEffectHandler>();
        statusEffectHandler?.Init();
    }
    //적 생성 시 호출
    public virtual void Spawn(Vector2Int[,] idxArr)
    {
        tileIndexArr = new Vector2Int[enemyData.size.x, enemyData.size.y];

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
        hpText.text = ((int)curHp).ToString();

        damageData.damage = enemyData.GetAttackPower();
    }

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

    public static Vector2Int[] FindPath(Vector2 start, Vector2 end)
    {
        Vector2 vec = end - start;
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
        return dirs;
    }



    // IHittable 인터페이스 구현 부
    public virtual void TakeDamage(DamageData damage)
    {
        if (curHp <= 0)
            return;

        damage.Applyed(hpPoint.transform.position);
        curHp = Mathf.Max(0, curHp - damage.damage);
        if (curHp > 1)
            hpText.text = ((int)curHp).ToString();
        else if (0 < curHp && curHp <= 1)
            hpText.text = "1";

        OnHpChanged();
        if (curHp < 1)
        {
            Reward();
            Destroy();
        }
    }

    public int face;
    public void UpdateFacing(Vector2 vec)
    {
        face = vec.x >= 0 ? 1 : -1;
        root.localScale = new Vector3(face, 1, 1);
    }

    public async virtual UniTask MoveTo(Vector2Int dir, float delaySec = 2)
    {
        Vector2Int[,] newTiles = MapManager.GetIndexArray(tileIndexArr, dir);
        MapManager.RegisterTile(newTiles, this); // 이동 중 다른 오브젝트가 점유 못하게 선점

        Vector2 dest = MapManager.TileIndexToCenterPosition(newTiles);
        UpdateFacing(dir);
        // await UniTask.Delay(3000);
        transform.DOMove(dest, 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                MapManager.ReleaseTile(tileIndexArr);//현재 위치 해제
                RegisterTile(newTiles); //이동한 위치 등록
            });

        await UniTask.Delay(TimeSpan.FromSeconds(delaySec));
    }

    protected virtual void OnHpChanged()
    {

    }
    public virtual void Reward()
    {

        Exp.Instantiate(transform.position, enemyData.exp, 1);
        EffectManager.Instance.Play(EffectType.StoneBreak, transform.position);
    }
    public virtual void Destroy()
    {
        ReleaseTile();
    }
    public bool CanHit()
    {
        return curHp > 0;
    }

    public void RegisterTile(Vector2Int[,] idxArr)
    {
        tileIndexArr = idxArr;
        MapManager.RegisterTile(idxArr, this);
    }

    public void ReleaseTile()
    {
        transform.DOKill();
        MapManager.ReleaseTile(tileIndexArr);
        gameObject.SetActive(false);
        GameEventBus.Publish(new EnemyDeadEvent(this));
    }


}

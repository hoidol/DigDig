using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public abstract class Slime : MonoBehaviour, IAllyUnit
{
    public string key;
    public int level;

    public SlimeMovement movement;
    // public SlimeAttackBehaviour attackBehaviour;

    public abstract float AttackSpeed();
    public abstract float AttackPower();

    public Transform rootTr;
    public SlimeData SlimeData => SlimeManager.Instance.GetSlimeData(key);
    public float AccumulatedDamage { get; set; }

    public float attackTimer;


    public void AccumulateDamage(float d)
    {
        AccumulatedDamage += d;
    }
    public Transform Transform => transform;
    public string Key => key;
    public AllyType AllyType => AllyType.Slime;
    public LayerMask targetLayerMask = 1 << 3;


    public Action<Transform> onTargetListener;
    public virtual void Awake()
    {
        movement = GetComponent<SlimeMovement>();
        if (rootTr == null)
            rootTr = transform.Find("Root");
    }

    public virtual void OnEnable()
    {
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
    }
    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        CheckTarget(e.enemy);
    }
    void OnDestroyedStoneEvent(DestroyedStoneEvent e)
    {
        CheckTarget(e.stone);
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

    public virtual void OnDisable()
    {

    }

    public virtual void Spawn(Vector2 pos, int lv)
    {
        transform.position = pos;
        this.level = lv;
    }

    public abstract string GetDescription(int level = 0);

    public abstract AllyBulletObject GetBullet();
    public virtual void Update()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer > AttackSpeed())
        {
            Fire(AttackDirecton());
        }
        if (targetTr == null)
        {
            rootTr.localScale = new Vector3(Character.Instance.AttackDir.x >= 0 ? 1 : -1, 1, 1);
        }
    }


    public Transform targetTr;
    public virtual Vector2 AttackDirecton()
    {
        targetTr = FindTarget();
        onTargetListener?.Invoke(targetTr);

        Vector2 fireDir = Character.Instance.moveJoystick.Direction;
        if (targetTr != null)
        {
            fireDir = (targetTr.position - transform.position).normalized;
            rootTr.localScale = new Vector3(fireDir.x >= 0 ? 1 : -1, 1, 1);
        }

        return fireDir;
    }


    public virtual void Fire(Vector2 dir)
    {
        AllyBulletObject baseBullet = GetBullet();
        if (baseBullet == null)
            return;

        baseBullet.transform.position = transform.position;
        baseBullet.Shoot(dir, AttackPower());
        attackTimer = 0;
    }

    public virtual async UniTask<(bool, string, int)> Merge(Slime target)
    {
        if (SlimeData.growth > 1 || target.SlimeData.growth > 1)
            return (false, null, 0);
        if (SlimeData.growth != target.SlimeData.growth)
            return (false, null, 0);
        if (level != target.level)
            return (false, null, 0);
        if (target.key != key)
            return (false, null, 0);
        if (level == 2 || target.level == 2)
            return (false, null, 0);

        string pickedSlimeKey = null;
        int lv = 0;
        if (SlimeData.growth == 1)
        {
            // if(level == 2)
            // {
            //     pickedSlimeKey = await SelectMergeSlimeCanvas.Instance.OpenCanvas(this, target);
            //     if (pickedSlimeKey == null)
            //         return (false, null, 0);
            // }
            // else if( level < 2)
            // {
            // }   
            UserSlime userSlime = UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes[Random.Range(0, UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes.Length)];
            pickedSlimeKey = userSlime.key;
            lv = level + 1;
        }
        else if (SlimeData.growth == 0)
        {
            UserSlime userSlime = UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes[Random.Range(0, UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes.Length)];
            pickedSlimeKey = userSlime.key;
        }

        return (true, pickedSlimeKey, lv);
    }
    //적 찾는 방식 설정
    public virtual Transform FindTarget()
    {
        return InGameUtil.FindTarget(transform.position, 10, targetLayerMask);
    }

}

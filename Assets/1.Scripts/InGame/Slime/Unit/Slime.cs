using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public abstract class Slime : MonoBehaviour, IAllyUnit
{
    public string key;
    public int level;

    public SlimeEnhanceAbility[] slimeEnhanceAbilities;
    public SlimeMovement movement;

    public Dictionary<StatType, SlimeStat> statDic = new Dictionary<StatType, SlimeStat>();

    public float AttackPower => statDic[StatType.AttackPower].value;
    public float AttackSpeed => statDic[StatType.AttackSpeed].value;
    public float AttackRange => statDic[StatType.AttackRange].value;

    // public float attackPower;
    // public float attackSpeed;
    // public float attackRange;


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
    public SlimeData slimeData;
    public List<Buff> activeBuffs = new List<Buff>();
    public virtual void Awake()
    {
        slimeEnhanceAbilities = GetComponentsInChildren<SlimeEnhanceAbility>();
        movement = GetComponent<SlimeMovement>();
        if (rootTr == null)
            rootTr = transform.Find("Root");

    }
     public UserSlime userSlime;
    public virtual void Spawn(Vector2 pos, int lv)
    {
        userSlime = UserManager.Instance.userSlimeManager.GetUserSlime(key);
        transform.position = pos;
        this.level = lv;

        slimeData = SlimeManager.Instance.GetSlimeData(key);

        statDic.Clear();
        statDic.Add(StatType.AttackPower, new SlimeStat(){ statType = StatType.AttackPower});
        statDic.Add(StatType.AttackSpeed, new SlimeStat(){ statType = StatType.AttackSpeed});
        statDic.Add(StatType.AttackRange, new SlimeStat(){ statType = StatType.AttackRange});

        UpdateSlime();
    }

    public void UpdateSlime()
    {
        statDic[StatType.AttackPower].value = slimeData.GetSlimeStat(StatType.AttackPower).value;
        statDic[StatType.AttackSpeed].value = slimeData.attackSpeed;
        statDic[StatType.AttackRange].value = slimeData.attackRange;

        foreach (var buff in activeBuffs)
        {
            var stat = statDic[buff.statType];
            stat.value = buff.Apply(stat.value);
        }
    }

    public SlimeEnhanceAbility GetSlimeEnhanceAbility(int lv)
    {
        for(int i = 0; i < slimeEnhanceAbilities.Length; i++)
        {
            if(slimeEnhanceAbilities[i].level == lv)
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


    public abstract string GetDescription(int level = 0);

    public abstract AllyBulletObject GetBullet();
    public virtual void Update()
    {
        
        attackTimer += Time.deltaTime * AttackSpeed / 50;
        if (attackTimer > AttackSpeed)
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
        baseBullet.Shoot(dir, AttackPower);
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
        return InGameUtil.FindTarget(transform.position, AttackRange, targetLayerMask);
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


}

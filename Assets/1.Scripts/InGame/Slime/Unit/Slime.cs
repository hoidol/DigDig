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
    public int mergeLevel;

    public SlimeEnhanceAbility[] slimeEnhanceAbilities;
    public SlimeMovement movement;

    public Dictionary<StatType, float> statDic = new Dictionary<StatType, float>();

    public float AttackPower => statDic[StatType.AttackPower];
    public float AttackSpeed => statDic[StatType.AttackSpeed];
    public float AttackRange => statDic[StatType.AttackRange];

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
    public SlimeEnhanceInfo slimeEnhanceInfo;
    public virtual void Spawn(Vector2 pos, int mLv)
    {
        userSlime = UserManager.Instance.userSlimeManager.GetUserSlime(key);

        slimeEnhanceInfo = slimeData.GetCommonSlimeEnhanceInfo(userSlime.enhanceLevel);
        transform.position = pos;
        this.mergeLevel = mLv;

        slimeData = SlimeManager.Instance.GetSlimeData(key);

        statDic.Clear();
        statDic.Add(StatType.AttackPower, 0);
        statDic.Add(StatType.AttackSpeed, 0);
        statDic.Add(StatType.AttackRange, 0);
        
        InitSlime();
        UpdateSlime();
    }

    public virtual void InitSlime()
    {

    }

    public virtual void InitSlimeStat()
    {        
        statDic[StatType.AttackPower] = slimeEnhanceInfo.GetSlimeStat(SlimeStatType.AttackPower).GetValue<float>(mergeLevel);
        statDic[StatType.AttackSpeed] =  slimeEnhanceInfo.GetSlimeStat(SlimeStatType.AttackSpeed).GetValue<float>(mergeLevel);
        statDic[StatType.AttackRange] = slimeEnhanceInfo.GetSlimeStat(SlimeStatType.AttackRange).GetValue<float>(mergeLevel); 
    }

    public virtual void UpdateSlime()
    {
        InitSlimeStat();

        foreach (var buff in activeBuffs)
        {
            statDic[buff.statType] = buff.Apply(statDic[buff.statType]);
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

    public abstract AllyBulletObject GetBullet();
    public virtual void Update()
    {
        if(AttackSpeed >= 0)
        {
            attackTimer += Time.deltaTime * AttackSpeed / 50;
            if (attackTimer > AttackSpeed)
            {
                Fire(AttackDirecton());
            }    
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
        if (mergeLevel != target.mergeLevel)
            return (false, null, 0);
        if (target.key != key)
            return (false, null, 0);
        if (mergeLevel == 2 || target.mergeLevel == 2)
            return (false, null, 0);

        string pickedSlimeKey = null;
        int lv = 0;
        if (SlimeData.growth == 1)
        {
            UserSlime userSlime = UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes[Random.Range(0, UserManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes.Length)];
            pickedSlimeKey = userSlime.key;
            lv = mergeLevel + 1;
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

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;

public class KingSlime : MonoSingleton<KingSlime>, IPicker, IAllyUnit, IHittable, ITileObject, IBuffable, IShotModifiable
{
    public const int COMBO_ATTACK_INTERVAL_MS = 70;

    public KingSlimeName kingslimeName;
    public Rigidbody2D rg;
    // public Joystick moveJoystick;
    // public Joystick attackJoystick;
    public KingSlimeStatManager statMgr;
    public Animator animator;
    public Transform bodyRootTr;
    public Transform bodyCenterTr;
    public CameraShake cameraShake;
    public float coinChance = 0.5f;

    public FindTargetType findTargetType = FindTargetType.Closest;

    public FindTargetType FindTargetType => findTargetType;

    public int exp;
    public int lv;

    bool levelUped;
    [SerializeField] int maxExp;

    [SerializeField] Transform hpPoint;
    public ItemInventory itemInventory; //패시브 스킬로 제공!
    public MinionInventory minionInventory;
    // public OreInventory oreInventory; //패시브 스킬로 제공!
    public TileChecker[] tileCheckers;
    public KingSlimeHealth health;
    // public CharacterMovement movement;
    public Weapon weapon;
    public ShotModifier shotModifier = new ShotModifier(); // 타일 아이템의 발사 방식 효과 (Weapon에서 사용)
    public ShotModifier ShotModifier => shotModifier;

    // 기존 호출부 변경 없이 유지되는 convenience 프로퍼티/메서드
    public float CurHp => health.curHp;
    public float healMultiplier { get => health.healMultiplier; set => health.healMultiplier = value; }
    public Transform rangeIndicatorTr;

    public Transform attackPoint => weapon.AttackPoint;
    // public Vector2 MoveDirection => movement.MoveDirection;
    // public float maxDistance => movement.maxDistance;
    // public int destroyCount;
    // public float distanceMaxDistanceDestroiedStone;
    // public float distanceMinDistanceDestroiedStone;
    public Vector2 AttackDir => weapon.GetAttackDirection();


    public Transform Transform => transform;
    public int coin;
    public int diceCoin;
    public float AccumulatedDamage { get; set; }
    public string key;
    public string Key => key;

    public AllyType AllyType => AllyType.Character;
    public AttackType AttackType => AttackType.Physic;

    public float MaxHp => statMgr.MaxHp;
    // public GameObject rangeObj;


    public void AccumulateDamage(float d)
    {
        AccumulatedDamage += d;
    }

    private void Awake()
    {
        rangeIndicatorTr.gameObject.SetActive(false);
        // moveJoystick = GameObject.Find("MoveJoystick").GetComponent<Joystick>();
        // attackJoystick = GameObject.Find("AttackJoystick").GetComponent<Joystick>();

        rg = GetComponentInChildren<Rigidbody2D>();
        itemInventory = GetComponentInChildren<ItemInventory>();
        minionInventory = GetComponentInChildren<MinionInventory>();
        // abilityInventory = GetComponentInChildren<AbilityInventory>();
        // statInventory = GetComponentInChildren<StatInventory>();
        health = GetComponentInChildren<KingSlimeHealth>();
        // movement = GetComponentInChildren<CharacterMovement>();
        weapon = GetComponentInChildren<Weapon>();


        tileCheckers = GetComponentsInChildren<TileChecker>();
        // subMachines = GetComponentsInChildren<SubMachine>();
    }


    void Start()
    {
        // rangeObj.SetActive(false);
        statMgr = new KingSlimeStatManager(this, kingslimeName);

        var statusEffectHandler = GetComponentInChildren<StatusEffectHandler>();
        health.Init(this, hpPoint, statusEffectHandler);
        // movement.Init(this, rg, animator, bodyRootTr);
        weapon.Init(this);

        UpdateCharacter();

    }

    void Update()
    {
        // if (!GameManager.Instance.isPlaying) return;

        // movement.Move();
        weapon.UpdateWeapon();

        // if (weapon.AttackDir != Vector2.zero)
        // {

        // }

#if UNITY_EDITOR
        // if (Input.GetKeyDown(KeyCode.L)) AddExp(10);
        if (Input.GetKeyDown(KeyCode.Minus))
            health.TakeDamage(new DamageData { damage = 40 });
#endif
    }

    public void PickUp(IPickable pickable)
    {
        pickable.PickedUp();
    }
    public void UpdateCharacter()
    {

        float preMaxHp = statMgr.MaxHp;
        statMgr.UpdateStat();
        float curMaxHp = statMgr.MaxHp;
        float diffMaxHp = curMaxHp - preMaxHp;
        if (diffMaxHp > 0)
        {
            AddHp(diffMaxHp, false);
        }
        // rangeObj.transform.localScale = Vector3.one * statMgr.AttackRange;
        GameEventBus.Publish(new KingSlimeUpdateEvent(this));
    }

    public void AddBuff(Buff buff)
    {
        statMgr.activeBuffs.Add(buff);
        UpdateCharacter();
    }

    public void RemoveBuff(Buff buff)
    {
        statMgr.activeBuffs.Remove(buff);
        UpdateCharacter();
    }

    // public void AddExp(int e)
    // {
    //     this.exp += e;
    //     if (exp >= GetMaxExp())
    //     {
    //         LevelUp();
    //         Time.timeScale = 0;
    //         InGame.LevelUpCanvas.Instance.OpenCanvas(() =>
    //         {
    //             Time.timeScale = 1;
    //             AddExp(0);
    //         });
    //     }
    //     GameEventBus.Publish(new ExpChangedEvent(exp, GetMaxExp()));
    // }
    public void SetFindTargetType(FindTargetType type)
    {
        findTargetType = type;
    }

    // void LevelUp()
    // {
    //     int remain = exp - GetMaxExp();
    //     exp = remain;
    //     lv++;
    //     levelUped = true;
    // }

    // public int GetMaxExp(int l = -1)
    // {
    //     if (l == -1) l = lv;
    //     if (maxExp == 0 || levelUped)
    //     {
    //         maxExp = 3 + l * 2;
    //         levelUped = false;
    //     }
    //     return maxExp;
    // }



    // 인벤토리가 가득 찼으면 교체 UI를 띄우고 결과를 기다림 (교체 시 ChangeItemCanvas에서 아이템 추가까지 처리)
    public async UniTask<bool> AddItem(string key, Tile tile = null)
    {
        statMgr.AddItem(key, 1);
        itemInventory.AddItem(key, tile);
        itemInventory.UpdateInventory();
        UpdateCharacter();
        return true;
    }

    // 타일 아이템 객체 하나 제거 (같은 key의 다른 타일 객체는 유지)
    public void RemoveItem(TileItem tileItem)
    {
        itemInventory.RemoveItem(tileItem); // curItems에서 먼저 빼야 UpdateInventory가 다른 객체를 건드리지 않음
        statMgr.AddItem(tileItem.key, -1);
        itemInventory.UpdateInventory();
        UpdateCharacter();
    }

    public Minion AddMinion(string key, int lv = 0)
    {
        Minion minion = MinionFieldController.Instance.Instantiate(key);
        minion.Spawn((Vector2)transform.position + UnityEngine.Random.insideUnitCircle, lv);

        statMgr.AddMinion(key);
        minionInventory.AddMinion(minion);
        minionInventory.UpdateInventory();

        GameEventBus.Publish(new SpawnMinionEvent(key, lv));
        UpdateCharacter();
        return minion;
    }

    public void RemoveMinion(Minion minion)
    {
        statMgr.RemoveMinion(minion.key);

        minionInventory.RemoveMinion(minion);
        minionInventory.UpdateInventory();
        UpdateCharacter();


        GameEventBus.Publish(new RemoveMinionEvent(minion.key));
    }

    public void AddCoin(int count)
    {
        coin += count;
        GameEventBus.Publish(new CoinEvent(coin));
    }

    public void AddDiceCoin(int count)
    {
        diceCoin += count;
        GameEventBus.Publish(new DiceCoinEvent(diceCoin));
    }

    public void TakeDamage(DamageData d) => health.TakeDamage(d);
    public void AddHp(float hp, bool showDmg = true) => health.AddHp(hp, showDmg);

    // public void AddLevelUpState(LevelUpStatType levelUpStatType, int lv)
    // {
    //     statMgr.AddLevelUpState(levelUpStatType, lv);

    //     switch (levelUpStatType)
    //     {
    //         case LevelUpStatType.FullHeal:
    //             AddHp(health.MaxHp);
    //             break;
    //     }

    //     UpdateCharacter();

    // }
    //플레이어에 의한 공격 Only
    // public void Attack(Vector2 dir) => weapon.Attack(dir);
    public KingSlimeBulletObject Shoot(BulletSpec b, Vector2 dir) => weapon.Shoot(b, dir);

    public bool CanHit()
    {
        return CurHp > 0;
    }

    public void ApplyStatusEffect(StatusEffect effect)
    {

    }

    public void StartTarget()
    {
        rangeIndicatorTr.gameObject.SetActive(true);
        rangeIndicatorTr.localScale = new Vector3(statMgr.AttackRange, statMgr.AttackRange, 1);
    }

    public void EndTarget()
    {

        rangeIndicatorTr.gameObject.SetActive(false);
    }

    public void StartDrag()
    {
        // rangeIndicatorTr.gameObject.SetActive(true);
        // rangeIndicatorTr.localScale = new Vector3(statMgr.AttackRange, statMgr.AttackRange, 1);
    }

    public void EndDrag(Tile tile)
    {
        // rangeIndicatorTr.gameObject.SetActive(false);
    }



}

//공격력(+ float), 체력(+ float), 치명타 확률(+ float), 치명타 피해량(+ float), 바운스(+ int), 이동속도(+ float 1초동안 얼만큼 가는지), 공격속도(+ float 1초동안 몇발 쏘는지)
public enum StatType
{
    AttackPower, //float
    MaxHp, //float
    AttackSpeed, //float 10초동안 몇발 쏘는지
    CritChance, //float
    CritPower, //float
    AttackRange,
    MaxMinionCount,
    StartGold, 
    WaveClearGold,
    BurnDPS,
    
    Count
}

public class BulletFiredEvent
{
    public BulletSpec bullet;
    public Vector2 dir;
}


public class KingSlimeUpdateEvent
{
    public KingSlime kingslime;
    public KingSlimeUpdateEvent(KingSlime kingslime) { this.kingslime = kingslime; }
}
public class CoinEvent
{
    public int curCoin;
    public CoinEvent(int curCoin) { this.curCoin = curCoin; }
}

public class DiceCoinEvent
{
    public int curCoin;
    public DiceCoinEvent(int curCoin) { this.curCoin = curCoin; }
}

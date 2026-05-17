using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;

public class Player : MonoSingleton<Player>, IPicker
{
    public const int COMBO_ATTACK_INTERVAL_MS = 70;

    public string key;
    public Rigidbody2D rg;
    public Joystick moveJoystick;
    public Joystick attackJoystick;
    public PlayerStatManager statMgr;
    public Color originColor = new Color(0.1921569f, 1f, 0.4705882f, 1f);
    public Color angerColor = new Color(1f, 0.1921569f, 0.1921569f, 1f);
    [SerializeField] Animator animator;
    public SpriteRenderer[] bodySprites;
    public Transform bodyRootTr;
    public Transform bodyCenterTr;
    public CameraShake cameraShake;
    public LayerMask pickLayer;
    [SerializeField] Transform hpPoint;

    public StatInventory statInventory;
    public ItemInventory itemInventory;
    public AbilityInventory abilityInventory;
    public List<MagmaBall> magmaBalls = new List<MagmaBall>();

    public PlayerHealth health;
    public PlayerMovement movement;
    public BaseGun weapon;

    // 기존 호출부 변경 없이 유지되는 convenience 프로퍼티/메서드
    public float curHp => health.curHp;
    public float healMultiplier { get => health.healMultiplier; set => health.healMultiplier = value; }
    public int curBulletCount => weapon.CurBulletCount;
    public bool isReloading => weapon.IsReloading;

    public Transform attackPoint => weapon.AttackPoint;
    public Vector2 MoveDirection => movement.MoveDirection;
    public float maxDistance => movement.maxDistance;
    public int destroyCount;
    public float distanceMaxDistanceDestroiedStone;
    public float distanceMinDistanceDestroiedStone;
    public Vector2 LastAttackDir => weapon.LastAttackDir;

    public Transform Transform => transform;

    public void TakeDamage(DamageData d) => health.TakeDamage(d);
    public void AddHp(float hp) => health.AddHp(hp);
    public void Attack(Vector2 dir, bool fromPlayer) => weapon.Attack(dir, fromPlayer);
    public PlayerBullet Shoot(Vector2 dir, Vector2 pos) => weapon.Shoot(dir, pos);
    public void QueueExtraShot(int count = 1) => weapon.QueueExtraShot(count);

    public int exp;
    public int lv;
    public int gold;
    bool levelUped;
    [SerializeField] int maxExp;

    private void Awake()
    {
        moveJoystick = GameObject.Find("MoveJoystick").GetComponent<Joystick>();
        attackJoystick = GameObject.Find("AttackJoystick").GetComponent<Joystick>();

        rg = GetComponentInChildren<Rigidbody2D>();
        magmaBalls = GetComponentsInChildren<MagmaBall>().ToList();
        itemInventory = GetComponentInChildren<ItemInventory>();
        abilityInventory = GetComponentInChildren<AbilityInventory>();
        statInventory = GetComponentInChildren<StatInventory>();
        var statusEffectHandler = GetComponentInChildren<StatusEffectHandler>();

        health = GetComponentInChildren<PlayerHealth>();
        movement = GetComponentInChildren<PlayerMovement>();
        weapon = GetComponentInChildren<BaseGun>();

        statMgr = new PlayerStatManager(this, key);

        health.Init(this, hpPoint, statusEffectHandler);
        movement.Init(this, rg, animator, bodyRootTr, moveJoystick);
        weapon.Init(this);
    }

    void Start()
    {
        // GameEventBus.Subscribe<UndergroundStartEvent>(OnUndergroundStart);
        UpdatePlayer();

        health.curHp = statMgr.MaxHp;
        health.RunRecover().Forget();

        movement.ResetOnUndergroundStart();
        weapon.ResetOnUndergroundStart();

        destroyCount = 0;
        distanceMaxDistanceDestroiedStone = 0f;
        distanceMinDistanceDestroiedStone = float.MaxValue;
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStone);
    }

    void OnDestroy()
    {
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStone);
    }

    void OnDestroyedStone(DestroyedStoneEvent e)
    {
        destroyCount++;
        float dist = Vector2.Distance(transform.position, e.oreStone.transform.position);
        if (dist > distanceMaxDistanceDestroiedStone) distanceMaxDistanceDestroiedStone = dist;
        if (dist < distanceMinDistanceDestroiedStone) distanceMinDistanceDestroiedStone = dist;
    }

    void Update()
    {
        if (GameManager.Instance.isClear) return;

        movement.Move();
        weapon.UpdateWeapon();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.L)) AddExp(10);
        if (Input.GetKeyDown(KeyCode.Minus))
            health.TakeDamage(new DamageData { damage = 40 });
#endif
    }

    public void PickUp(IPickable pickable)
    {
        pickable.PickedUp();
        AddGold(1);
    }

    public void AddGold(int gold)
    {
        this.gold += gold;
        GameEventBus.Publish(new GoldChangedEvent(this.gold, gold));
    }

    public void UpdatePlayer()
    {
        float preMaxHp = statMgr.MaxHp;
        statMgr.UpdateStat();
        float curMaxHp = statMgr.MaxHp;
        float diffMaxHp = curMaxHp - preMaxHp;
        if (diffMaxHp > 0)
            AddHp(diffMaxHp);
        GameEventBus.Publish(new PlayerUpdateEvent(this));
    }

    public void AddBuff(Buff buff)
    {
        statMgr.activeBuffs.Add(buff);
        UpdatePlayer();
    }

    public void RemoveBuff(Buff buff)
    {
        statMgr.activeBuffs.Remove(buff);
        UpdatePlayer();
    }

    public void AddExp(int e)
    {
        this.exp += e;
        if (exp >= GetMaxExp())
        {
            LevelUp();
            Time.timeScale = 0;
            AbilityCanvas.Instance.OpenCanvas(() =>
            {
                Time.timeScale = 1;
                AddExp(0);
            });
        }
        GameEventBus.Publish(new ExpChangedEvent(exp, GetMaxExp()));
    }

    void LevelUp()
    {
        int remain = exp - GetMaxExp();
        exp = remain;
        lv++;
        levelUped = true;
    }

    public int GetMaxExp(int l = -1)
    {
        if (l == -1) l = lv;
        if (maxExp == 0 || levelUped)
        {
            maxExp = 3 + l * 3;
            levelUped = false;
        }
        return maxExp;
    }
}

[System.Serializable]
public class PlayerStatManager
{
    [SerializeField] List<PlayerStat> statList = new();
    public Dictionary<StatType, PlayerStat> statDic = new Dictionary<StatType, PlayerStat>();
    public List<Buff> activeBuffs = new List<Buff>();

    public float MaxHp => statDic[StatType.MaxHp].value;
    public float AttackPower => statDic[StatType.AttackPower].value;
    public float MoveSpeed => statDic[StatType.MoveSpeed].value;
    public float RecoveryHp => statDic[StatType.RecoveryHp].value;
    public float AttackSpeed => statDic[StatType.AttackSpeed].value;
    public float AttackRange => statDic[StatType.AttackRange].value;
    public float CritChance => statDic[StatType.CritChance].value;
    public float CritPower => statDic[StatType.CritPower].value;
    public float ReloadTime => statDic[StatType.ReloadTime].value;
    public float ReloadSpeed => statDic[StatType.ReloadSpeed].value;
    public float MagicPower => statDic[StatType.MagicPower].value;
    public float Lucky => statDic[StatType.Lucky].value;
    public float PickUpRange => statDic[StatType.PickUpRange].value;
    public int BulletCount => Mathf.Max(1, (int)statDic[StatType.BulletCount].value);

    PlayerData playerData;
    Player player;
    //StatUpAbilityData[] statUpAbilityDatas;

    public PlayerStatManager(Player p, string key)
    {
        player = p;
        playerData = Resources.Load<PlayerData>($"PlayerData/{key}");

        statDic.Clear();
        statList.Clear();
        for (int i = 0; i < (int)StatType.Count; i++)
        {
            var ps = new PlayerStat { statType = (StatType)i };
            statList.Add(ps);
            statDic.Add(ps.statType, ps);
        }
        // statUpAbilityDatas = Resources.LoadAll<StatUpAbilityData>("AbilityData/StatUpAbilityData");
        Init();
    }

    void Init()
    {
        for (int i = 0; i < (int)StatType.Count; i++)
        {
            StatType statType = (StatType)i;
            statDic[statType].value = playerData.GetPlayerStat(statType).value;
        }
    }

    public void UpdateStat()
    {
        Init();
        //for (int i = 0; i < statUpAbilityDatas.Length; i++)
        //{
        //    StatUpAbility pAbility = player.abilityInventory.GetAbility(statUpAbilityDatas[i].key) as StatUpAbility;
        //    if (pAbility == null || pAbility.count <= 0) continue;
        //    var stat = statDic[pAbility.statType];
        //    stat.value = pAbility.Apply(stat.value, pAbility.count);
        //}
        for (int i = 0; i < player.statInventory.ownStats.Count; i++)
        {
            Stat stat = player.statInventory.ownStats[i];
            if (stat.count <= 0) continue;
            StatData statData = StatData.GetStatData(stat.statType.ToString());

            var playerStat = statDic[stat.statType];
            playerStat.value = statData.Apply(playerStat.value, stat.count);
        }



        foreach (var buff in activeBuffs)
        {
            var stat = statDic[buff.statType];
            stat.value = buff.Apply(stat.value);
        }
    }

    public static StatType UpgradeTypeToStatType(UpgradeType upgradeType)
    {
        if (Enum.TryParse<StatType>(upgradeType.ToString(), out StatType playerStatType))
            return playerStatType;
        return StatType.Count;
    }
}

[System.Serializable]
public class PlayerStat
{
    public StatType statType;
    public float value;
}

public enum StatType
{
    AttackPower,
    MaxHp,
    RecoveryHp,
    AttackSpeed,
    MoveSpeed,
    AttackRange,
    AngerTime,
    CritChance,
    CritPower,
    BulletCount,
    MagicPower,
    ReloadSpeed,
    PickUpRange,
    Lucky,
    ReloadTime,
    Count
}

public class BulletFiredEvent
{
    public bool fromPlayer { get; set; }
    public int currentBulletCount { get; set; }
    public int maxBulletCount { get; set; }
    public Vector2 dir;
}

public class ReloadEvent { }

public class PlayerUpdateEvent
{
    public Player player;
    public PlayerUpdateEvent(Player player) { this.player = player; }
}

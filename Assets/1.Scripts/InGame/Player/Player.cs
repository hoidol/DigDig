using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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

    public int exp;
    public int lv;

    bool levelUped;
    [SerializeField] int maxExp;
    public int bounce;
    [SerializeField] Transform hpPoint;

    public StatInventory statInventory;
    public ItemInventory itemInventory; //패시브 스킬로 제공!
    public List<MagmaBall> magmaBalls = new List<MagmaBall>();
    public TileChecker[] tileCheckers;
    public PlayerHealth health;
    public PlayerMovement movement;
    public BaseGun weapon;

    // 기존 호출부 변경 없이 유지되는 convenience 프로퍼티/메서드
    public float curHp => health.curHp;
    public float healMultiplier { get => health.healMultiplier; set => health.healMultiplier = value; }

    public bool isReloading => weapon.IsReloading;

    public Transform attackPoint => weapon.AttackPoint;
    public Vector2 MoveDirection => movement.MoveDirection;
    public float maxDistance => movement.maxDistance;
    public int destroyCount;
    public float distanceMaxDistanceDestroiedStone;
    public float distanceMinDistanceDestroiedStone;
    public Vector2 LastAttackDir => weapon.LastAttackDir;

    public Transform Transform => transform;
    public BulletSubTool[] bulletSubTools;
    public BulletSubTool GetBulletSubTool(string key)
    {
        return bulletSubTools.FirstOrDefault(t => t.key == key);
    }

    private void Awake()
    {
        moveJoystick = GameObject.Find("MoveJoystick").GetComponent<Joystick>();
        attackJoystick = GameObject.Find("AttackJoystick").GetComponent<Joystick>();

        rg = GetComponentInChildren<Rigidbody2D>();
        magmaBalls = GetComponentsInChildren<MagmaBall>().ToList();
        itemInventory = GetComponentInChildren<ItemInventory>();
        // abilityInventory = GetComponentInChildren<AbilityInventory>();

        statInventory = GetComponentInChildren<StatInventory>();

        health = GetComponentInChildren<PlayerHealth>();
        movement = GetComponentInChildren<PlayerMovement>();
        weapon = GetComponentInChildren<BaseGun>();


        tileCheckers = GetComponentsInChildren<TileChecker>();
        statMgr = new PlayerStatManager(this, key);

    }


    void Start()
    {
        var statusEffectHandler = GetComponentInChildren<StatusEffectHandler>();
        health.Init(this, hpPoint, statusEffectHandler);
        movement.Init(this, rg, animator, bodyRootTr, moveJoystick);
        weapon.Init(this);

        UpdatePlayer();

        health.curHp = statMgr.MaxHp;
        health.RunRecover().Forget();

        movement.ResetOnUndergroundStart();
        weapon.ResetOnUndergroundStart();

        bounce = 0;
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
    }

    public void AddBounce(int c)
    {
        this.bounce += c;

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

    public void LevelUpBullet(string key)
    {
        statMgr.LevelUpBullet(key);
        UpdatePlayer();
    }

    public void LevelUpItem(string key)
    {
        statMgr.LevelUpBullet(key);
        UpdatePlayer();
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
            LevelUpCanvas.Instance.OpenCanvas(() =>
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
            maxExp = 3 + l * 5;
            levelUped = false;
        }
        return maxExp;
    }



    public void TakeDamage(DamageData d) => health.TakeDamage(d);
    public void AddHp(float hp) => health.AddHp(hp);


    //플레이어에 의한 공격 Only
    public void Attack(Vector2 dir) => weapon.Attack(dir);
    public PlayerBulletObject Shoot(Bullet b, Vector2 dir, Vector2 pos) => weapon.Shoot(b, dir, pos);

    // public void QueueExtraShot(int count = 1) => weapon.QueueExtraShot(count);

}

[System.Serializable]
public class PlayerStatManager
{
    [SerializeField] List<PlayerStat> statList = new();
    public Dictionary<StatType, PlayerStat> statDic = new Dictionary<StatType, PlayerStat>();
    public Dictionary<string, PlayerBulletStat> bulletStatDic = new Dictionary<string, PlayerBulletStat>();
    public Dictionary<string, PlayerItemStat> itemStatDic = new Dictionary<string, PlayerItemStat>();
    public List<Buff> activeBuffs = new List<Buff>();

    public float MaxHp => statDic[StatType.MaxHp].value;
    public float AttackPower => statDic[StatType.AttackPower].value;
    public float MoveSpeed => statDic[StatType.MoveSpeed].value;
    public float RecoveryHp => statDic[StatType.RecoveryHp].value;
    public float AttackSpeed => statDic[StatType.AttackSpeed].value;
    public float CritChance => statDic[StatType.CritChance].value;
    public float CritPower => statDic[StatType.CritPower].value;
    public float ReloadSpeed => statDic[StatType.ReloadSpeed].value;
    public float PickUpRange => statDic[StatType.PickUpRange].value;
    public float AmmoDuration => statDic[StatType.AmmoDuration].value;
    public float AmmoEfficiency => statDic[StatType.AmmoEfficiency].value;

    PlayerData playerData;
    Player player;

    StatType[] usingStatTypes =
    {
        StatType.MaxHp,StatType.AttackPower,StatType.MoveSpeed,
StatType.RecoveryHp,StatType.AttackSpeed,StatType.CritChance,
StatType.CritPower,
StatType.ReloadSpeed,
StatType.PickUpRange,
StatType.AmmoDuration //총알 지속시간
,StatType.AmmoEfficiency
    };
    public PlayerStatManager(Player p, string key)
    {
        player = p;
        playerData = Resources.Load<PlayerData>($"PlayerData/{key}");

        statDic.Clear();
        statList.Clear();

        for (int i = 0; i < usingStatTypes.Length; i++)
        {
            var ps = new PlayerStat { statType = usingStatTypes[i] };
            statList.Add(ps);
            statDic.Add(ps.statType, ps);
        }

        Init();
    }

    void Init()
    {
        for (int i = 0; i < usingStatTypes.Length; i++)
        {
            StatType statType = usingStatTypes[i];
            statDic[statType].value = playerData.GetPlayerStat(statType).value;
        }

        for (int i = 0; i < UserManager.Instance.userData.equiptedBullets.Length; i++)
        {
            bulletStatDic.Add(UserManager.Instance.userData.equiptedBullets[i].key,
            new PlayerBulletStat()
            {
                key = UserManager.Instance.userData.equiptedBullets[i].key,
                lv = 0
            });
        }


    }

    public void UpdateStat()
    {
        Init();
        for (int i = 0; i < player.statInventory.ownStats.Count; i++)
        {
            Stat stat = player.statInventory.ownStats[i];
            if (stat.lv <= 0) continue;
            StatData statData = StatData.GetStatData(stat.statType.ToString());

            var playerStat = statDic[stat.statType];
            playerStat.value = statData.Apply(playerStat.value, stat.lv);
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
    public void LevelUpBullet(string key)
    {
        if (!bulletStatDic.ContainsKey(key))
        {
            bulletStatDic.Add(key, new PlayerBulletStat()
            {
                key = key,
                lv = 0
            });
        }
        bulletStatDic[key].lv++;
    }
    public void LevelUpItem(string key)
    {
        if (!itemStatDic.ContainsKey(key))
        {
            itemStatDic.Add(key, new PlayerItemStat()
            {
                key = key,
                lv = 0
            });
        }
        bulletStatDic[key].lv++;
    }
}

[System.Serializable]
public class PlayerStat
{
    public StatType statType;
    public float value;
}

[System.Serializable]
public class PlayerBulletStat
{
    public string key;
    public int lv;
}

[System.Serializable]
public class PlayerItemStat
{
    public string key;
    public int lv;
}

public enum StatType
{
    AttackPower,
    MaxHp,
    RecoveryHp,
    AttackSpeed,
    MoveSpeed,
    CritChance,
    CritPower,
    // BulletCount,
    ReloadSpeed,
    PickUpRange,
    // ReloadTime,
    AmmoDuration,
    AmmoEfficiency, //튕기는 때 데미지 감소량을 줄어듦

    Count
}

public class BulletFiredEvent
{
    public Bullet bullet;
    public Vector2 dir;
}

public class ReloadEvent { }

public class PlayerUpdateEvent
{
    public Player player;
    public PlayerUpdateEvent(Player player) { this.player = player; }
}

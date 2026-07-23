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
    // public Color originColor = new Color(0.1921569f, 1f, 0.4705882f, 1f);
    // public Color angerColor = new Color(1f, 0.1921569f, 0.1921569f, 1f);
    [SerializeField] Animator animator;
    // public SpriteRenderer[] bodySprites;
    public Transform bodyRootTr;
    public Transform bodyCenterTr;
    public CameraShake cameraShake;

    public int exp;
    public int lv;

    bool levelUped;
    [SerializeField] int maxExp;
    public int bounce;
    [SerializeField] Transform hpPoint;

    // public StatInventory statInventory;
    public ItemInventory itemInventory; //패시브 스킬로 제공!
    public TileChecker[] tileCheckers;
    public PlayerHealth health;
    public PlayerMovement movement;
    public BaseGun weapon;

    // 기존 호출부 변경 없이 유지되는 convenience 프로퍼티/메서드
    public float curHp => health.curHp;
    public float healMultiplier { get => health.healMultiplier; set => health.healMultiplier = value; }


    public Transform attackPoint => weapon.AttackPoint;
    public Vector2 MoveDirection => movement.MoveDirection;
    public float maxDistance => movement.maxDistance;
    public int destroyCount;
    public float distanceMaxDistanceDestroiedStone;
    public float distanceMinDistanceDestroiedStone;
    public Vector2 LastAttackDir => weapon.LastAttackDir;
    public Vector2 CurAttackDir => weapon.LastAttackDir;

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
        itemInventory = GetComponentInChildren<ItemInventory>();
        // abilityInventory = GetComponentInChildren<AbilityInventory>();
        // statInventory = GetComponentInChildren<StatInventory>();
        health = GetComponentInChildren<PlayerHealth>();
        movement = GetComponentInChildren<PlayerMovement>();
        weapon = GetComponentInChildren<BaseGun>();


        tileCheckers = GetComponentsInChildren<TileChecker>();
    }


    void Start()
    {
        statMgr = new PlayerStatManager(this, key);

        var statusEffectHandler = GetComponentInChildren<StatusEffectHandler>();
        health.Init(this, hpPoint, statusEffectHandler);
        movement.Init(this, rg, animator, bodyRootTr);
        weapon.Init(this);

        UpdatePlayer();


        movement.Restart();

        bounce = 0;
        destroyCount = 0;
        distanceMaxDistanceDestroiedStone = MapManager.MIN_RANGE_RADIUS;
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
        float dist = Vector2.Distance(transform.position, e.stone.transform.position);
        if (dist > distanceMaxDistanceDestroiedStone) distanceMaxDistanceDestroiedStone = dist;
        if (dist < distanceMinDistanceDestroiedStone) distanceMinDistanceDestroiedStone = dist;
    }

    void Update()
    {
        if (!GameManager.Instance.isPlaying) return;

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
        itemInventory.UpdateInventory();

        float preMaxHp = statMgr.MaxHp;
        statMgr.UpdateStat();
        float curMaxHp = statMgr.MaxHp;
        float diffMaxHp = curMaxHp - preMaxHp;
        if (diffMaxHp > 0)
            AddHp(diffMaxHp);
            
        GameEventBus.Publish(new PlayerUpdateEvent(this));
    }

    // public void LevelUpBullet(string key)
    // {
    //     statMgr.LevelUpBullet(key);
    //     UpdatePlayer();
    // }


    public void MergeBullet(MergeBulletData mergeBulletData)
    {
        //weapon.SetBullet(mergeBulletData.resultBulletKey);
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
            maxExp = 5 + l * 3;
            levelUped = false;
        }
        return maxExp;
    }



    public void AddItem(string key,int count=1)
    {
        statMgr.AddItem(key,count);
        
        UpdatePlayer();
    }

    public void TakeDamage(DamageData d) => health.TakeDamage(d);
    public void AddHp(float hp, bool showDmg = true) => health.AddHp(hp, showDmg);


    //플레이어에 의한 공격 Only
    // public void Attack(Vector2 dir) => weapon.Attack(dir);
    public PlayerBulletObject Shoot(Bullet b, Vector2 dir) => weapon.Shoot(b, dir);

    // public void QueueExtraShot(int count = 1) => weapon.QueueExtraShot(count);

}

[System.Serializable]
public class PlayerStatManager
{
    [SerializeField] List<PlayerStat> statList = new();
    public Dictionary<StatType, PlayerStat> statDic = new Dictionary<StatType, PlayerStat>();
    // public Dictionary<string, PlayerBulletStat> bulletStatDic = new Dictionary<string, PlayerBulletStat>();
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

        // for (int i = 0; i < UserManager.Instance.userBulletManager.userBulletData.equiptedBullets.Length; i++)
        // {
        //     // Debug.Log($"Player StatManager Init() {UserManager.Instance.userData.equiptedBullets[i].key}");
        //     bulletStatDic.Add(UserManager.Instance.userBulletManager.userBulletData.equiptedBullets[i].key,
        //     new PlayerBulletStat()
        //     {
        //         key = UserManager.Instance.userBulletManager.userBulletData.equiptedBullets[i].key,
        //         lv = 1
        //     });
        // }

        Reset();
    }

    void Reset()
    {
        for (int i = 0; i < usingStatTypes.Length; i++)
        {
            StatType statType = usingStatTypes[i];
            statDic[statType].value = playerData.GetPlayerStat(statType).value;
        }
    }
    public void UpdateStat()
    {
        Reset();
        // for (int i = 0; i < player.statInventory.ownStats.Count; i++)
        // {
        //     Stat stat = player.statInventory.ownStats[i];
        //     if (stat.lv <= 0) continue;
        //     StatData statData = StatData.GetStatData(stat.statType.ToString());

        //     var playerStat = statDic[stat.statType];
        //     playerStat.value = statData.Apply(playerStat.value, stat.lv);
        // }



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
    public void AddItem(string key,int count)
    {
        if (!itemStatDic.ContainsKey(key))
        {
            itemStatDic.Add(key, new PlayerItemStat()
            {
                key = key,
                count = 0
            });
        }
        itemStatDic[key].count += count;
        if(itemStatDic[key].count <= 0)
        {
            //아이템 제거하기
        }
    }

    // public void LevelUpBullet(string key)
    // {
    //     bulletStatDic[key].lv++;
    // }
    public PlayerItemStat GetPlayerItemStat(string key)
    {
        return itemStatDic[key];
    }
    // public void LevelUpItem(string key)
    // {
    //     itemStatDic[key].count++;
    // }
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
    public int count;
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

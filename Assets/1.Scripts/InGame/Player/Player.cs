using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;

public class Player : MonoSingleton<Player>, IPicker
{

    public Transform Transform
    {
        get { return transform; }
    }
    public Transform attackPoint;
    public string key;
    public Rigidbody2D rg;
    public Joystick moveJoystick;
    public Joystick attackJoystack;
    public PlayerStatManager playerStatMgr;
    public float angerAmount;
    public Color originColor = new Color(0.1921569f, 1f, 0.4705882f, 1f);
    public Color angerColor = new Color(1f, 0.1921569f, 0.1921569f, 1f); //FF3131 //분노
    [SerializeField] Animator animator;
    public SpriteRenderer[] bodySprites;
    public Transform bodyRootTr;
    public Transform bodyCenterTr;

    // public float maxHp;
    public float curHp;

    public int exp;
    public int lv;
    public float attackTimer = 0f;
    public int gold;
    public List<MagmaBall> magmaBalls = new List<MagmaBall>();
    //public MagmaRotation magmaRotation;
    Camera mainCamara;
    public Inventory inventory;
    public List<PlayerAbility> playerAbilities = new List<PlayerAbility>();

    public LayerMask pickLayer;

    private void Awake()
    {
        mainCamara = Camera.main;
        moveJoystick = GameObject.Find("MoveJoystick").GetComponent<Joystick>();
        attackJoystack = GameObject.Find("AttackJoystick").GetComponent<Joystick>();
        rg = GetComponentInChildren<Rigidbody2D>();
        magmaBalls = GetComponentsInChildren<MagmaBall>().ToList();
        inventory = GetComponentInChildren<Inventory>();
        playerStatMgr = new PlayerStatManager(this, key);
    }

    void Start()
    {
        SetBodyColor(originColor);
        playerStatMgr.UpdateStat();
        curHp = playerStatMgr.MaxHp;

    }
    void SetBodyColor(Color color)
    {
        for (int i = 0; i < bodySprites.Length; i++)
        {
            bodySprites[i].color = color;
        }

        magmaBalls.ForEach(ball => ball.SetColor(color));
    }
    bool isAnger;
    void Move()
    {
        rg.linearVelocity = moveJoystick.Direction * playerStatMgr.MoveSpeed;

#if UNITY_EDITOR
        // 키보드 조작으로 이동하게 처리
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 moveDir = new Vector2(x, y).normalized;
        if (moveDir.magnitude > 0.1f)
        {
            if (moveDir.x >= 0)
                bodyRootTr.localScale = new Vector3(1, 1, 1);
            else
                bodyRootTr.localScale = new Vector3(-1, 1, 1);
            animator.SetBool("Running", true);
        }
        else
        {
            animator.SetBool("Running", false);
        }
        rg.linearVelocity = moveDir * playerStatMgr.MoveSpeed;

#else

        if (moveJoystick.Direction.magnitude > 0.1f)
        {
            if (moveJoystick.Direction.x >= 0)
                bodyRootTr.localScale = new Vector3(1, 1, 1);
            else
                bodyRootTr.localScale = new Vector3(-1, 1, 1);
            animator.SetBool("Running", true);
        }
        else
        {
            animator.SetBool("Running", false);
        }
#endif
    }
    void Update()
    {
        Move();
        UpdateAttack();

        angerAmount -= Time.deltaTime;
        if (angerAmount < 0)
            angerAmount = 0;


    }

    public void UpdateAttack()
    {
        attackTimer += Time.deltaTime;
#if UNITY_EDITOR
        if (attackTimer >= playerStatMgr.AttackSpeed)
        {

            // attackPoint에서 마우스 위치로 발사
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = mainCamara.WorldToScreenPoint(attackPoint.position).z;
            Vector3 worldMousePos = mainCamara.ScreenToWorldPoint(mousePosition);
            Vector2 dir = (worldMousePos - attackPoint.position).normalized;
            Attack(dir, true);
        }
#else
        if (attackJoystack.Direction.magnitude > 0 && attackTimer >= playerStatMgr.AttackSpeed)
        {
            
            Attack(attackJoystack.Direction.normalized);
        }
#endif
    }
    public void AddGold(int gold)
    {
        this.gold += gold;
        GameEventBus.Publish(new GoldChangedEvent(gold));
    }


    public void AddHp(float hp)
    {
        curHp += hp;
        if (curHp > playerStatMgr.MaxHp)
        {
            curHp = playerStatMgr.MaxHp;
        }
    }
    StatusEffectHandler statusEffectHandler;
    public void TakeDamage(float damage)
    {
        if (statusEffectHandler != null && statusEffectHandler.TryBlock()) return;
        curHp -= damage;
        if (curHp < 0)
        {
            GameManager.Instance.EndGame(false);
        }
    }

    public void Attack(Vector2 dir, bool fromPlayer)
    {
        var bullet = Shoot(dir);//한발쏨

        foreach (var e in inventory.equippedItems.OfType<IAttackItem>())
            e.OnAttack(this, dir);

        if (fromPlayer)
            RunComboAttacks(dir).Forget();

        GameEventBus.Publish(new BulletFiredEvent(bullet));

        attackTimer = 0f;

        if (dir.x >= 0)
            bodyRootTr.localScale = new Vector3(1, 1, 1);
        else
            bodyRootTr.localScale = new Vector3(-1, 1, 1);

        angerAmount += 5f;
        if (angerAmount > 100f && !isAnger)
        {
            angerAmount = 100f;
            isAnger = true;
            StartCoroutine(CoAnger());
        }
    }

    async UniTaskVoid RunComboAttacks(Vector2 dir)
    {
        var comboItems = inventory.equippedItems.OfType<IComboAttackItem>().ToList();
        foreach (var e in comboItems)
            await e.OnAttack(this, dir);
    }

    public PlayerBullet Shoot(Vector2 dir)
    {
        var bullet = PlayerBullet.Instantiate();
        bullet.ClearBehaviors();
        bullet.transform.position = attackPoint.position;

        //총알에 IBulletItem 타입의 아이템 능력 적용
        foreach (var e in inventory.equippedItems.OfType<IBulletItem>())
            e.OnBulletFired(bullet);

        bullet.Shoot(dir, playerStatMgr.AttackPower);
        return bullet;
    }





    IEnumerator CoAnger()
    {
        SetBodyColor(angerColor);
        yield return new WaitForSeconds(10f);
        isAnger = false;
        angerAmount = 0;
        SetBodyColor(originColor);
    }

    public void PickUp(IPickable pickable)
    {
        pickable.PickedUp();

        AddGold(1);
    }

    public void AddAbility(string key)
    {
        PlayerAbility playerAbility = playerAbilities.Where(a => a.key == key).FirstOrDefault();
        if (playerAbility == null)
        {
            playerAbility = new PlayerAbility { key = key };
            playerAbilities.Add(playerAbility);
        }

        playerAbility.count++;

        UpdatePlayer();
    }

    public PlayerAbility GetPlayerAbility(string key)
    {
        return playerAbilities.Where(a => a.key == key).FirstOrDefault();
    }


    public void Upgrade(UpgradeType upgradeType)
    {
        if (upgradeType == UpgradeType.Exp)
        {
            AddExp(3);
            return;
        }

        playerStatMgr.Upgrade(upgradeType);
        UpdatePlayer();
    }

    void UpdatePlayer()
    {
        #region  Upgrade에 따른 능력치 적용
        float preMaxHp = playerStatMgr.MaxHp;
        playerStatMgr.UpdateStat();
        float curMaxHp = playerStatMgr.MaxHp;
        float diffMaxHp = curMaxHp - preMaxHp;
        if (diffMaxHp > 0)
        {
            AddHp(diffMaxHp);
        }

        #endregion



    }


    public void AddExp(int e)
    {
        this.exp += e;
        if (exp >= GetMaxExp())
        {
            Debug.Log("LevelUp!");
            LevelUp();
            Time.timeScale = 0;
            AbilityCanvas.Instance.OpenCanvas(() =>
            {
                Time.timeScale = 1;
                AddExp(0);//레벨업 후에도 경험치가 MaxExp 높을때 처리
            });
        }

        GameEventBus.Publish(new ExpChangedEvent(exp, GetMaxExp()));
    }
    bool levelUped;
    void LevelUp()
    {
        int remain = exp - GetMaxExp();
        exp = remain;
        lv++;
        levelUped = true;
        //강화 UI 활성화
    }

    [SerializeField] int maxExp;
    public int GetMaxExp(int l = -1)
    {
        if (l == -1)
            l = lv;

        if (maxExp == 0 || levelUped)
        {
            maxExp = 10 + l * 5;
            levelUped = false;
        }

        return maxExp;
    }
}


// [System.Serializable]
// public class AngerStep
// {
//     public Color color;
//     public float minAngerAmount;
//     public float maxAngerAmount;
//     public float breatheTime;
//     public float breatheScale;
// }

[System.Serializable]
public class PlayerAbility
{
    public string key;
    public int count;
}



//[System.Serializable]
public class PlayerStatManager
{
    public Dictionary<StatType, PlayerStat> statDic = new Dictionary<StatType, PlayerStat>();
    public List<Buff> activeBuffs = new List<Buff>();

    public void AddBuff(Buff buff)
    {
        activeBuffs.Add(buff);
        UpdateStat();
    }

    public void RemoveBuff(Buff buff)
    {
        activeBuffs.Remove(buff);
        UpdateStat();
    }
    public float MaxHp => statDic[StatType.MaxHp].value;
    public float AttackPower => statDic[StatType.AttackPower].value;
    public float MoveSpeed => statDic[StatType.MoveSpeed].value;
    public float RecoveryHp => statDic[StatType.RecoveryHp].value;
    public float AttackSpeed => statDic[StatType.AttackSpeed].value;
    public float AttackRange => statDic[StatType.AttackRange].value;
    public float CritChance => statDic[StatType.CritChance].value;
    public float CritPower => statDic[StatType.CritPower].value;

    PlayerData playerData;
    Player player;
    StatUpAbilityData[] statUpAbilityDatas;
    public PlayerStatManager(Player p, string key)
    {
        player = p;
        playerData = Resources.Load<PlayerData>($"PlayerData/{key}");
        if (playerData == null)
            Debug.Log("if(playerData == null)");

        statDic.Clear();
        PlayerStat[] playerStats = new PlayerStat[(int)StatType.Count];
        for (int i = 0; i < (int)StatType.Count; i++)
        {
            playerStats[i] = new PlayerStat();
            playerStats[i].statType = (StatType)i;
            statDic.Add(playerStats[i].statType, playerStats[i]);
        }
        statUpAbilityDatas = Resources.LoadAll<StatUpAbilityData>("AbilityData/StatUpAbilityData");
        Init();
    }
    void Init()
    {
        statDic[StatType.MaxHp].value = playerData.GetPlayerStat(StatType.MaxHp).value;
        statDic[StatType.AttackPower].value = playerData.GetPlayerStat(StatType.AttackPower).value;
        statDic[StatType.MoveSpeed].value = playerData.GetPlayerStat(StatType.MoveSpeed).value;
        statDic[StatType.AttackSpeed].value = playerData.GetPlayerStat(StatType.AttackSpeed).value;
        statDic[StatType.AttackRange].value = playerData.GetPlayerStat(StatType.AttackRange).value;
        statDic[StatType.RecoveryHp].value = playerData.GetPlayerStat(StatType.RecoveryHp).value;
    }
    public void Upgrade(UpgradeType upgradeType)
    {
        statDic[UpgradeTypeToStatType(upgradeType)].upgradeLv++;
        UpdateStat();
    }
    public void UpdateStat()
    {
        Init();

        #region  Upgrade 따른 능력치 적용
        // statDic[StatType.AttackPower].value = AttackPower + UpgradeData.GetUpgradeData(UpgradeType.AttackPower).GetValue(statDic[StatType.AttackPower].upgradeLv);
        // statDic[StatType.MaxHp].value = MaxHp + UpgradeData.GetUpgradeData(UpgradeType.MaxHp).GetValue(statDic[StatType.MaxHp].upgradeLv);
        // statDic[StatType.RecoveryHp].value = RecoveryHp + UpgradeData.GetUpgradeData(UpgradeType.RecoveryHp).GetValue(statDic[StatType.RecoveryHp].upgradeLv);

        #endregion

        #region  Ability 따른 능력치 적용
        //statDic[StatType.MaxHp].value = player.GetPlayerAbility("HpUp").count

        for (int i = 0; i < statUpAbilityDatas.Length; i++)
        {
            var data = statUpAbilityDatas[i];
            PlayerAbility pAbility = player.GetPlayerAbility(statUpAbilityDatas[i].key);

            if (pAbility == null || pAbility.count <= 0)
                continue;
            var stat = statDic[data.statType];
            stat.value = data.Apply(stat.value, pAbility.count);
        }

        #endregion

        #region Buff 적용
        foreach (var buff in activeBuffs)
        {
            var stat = statDic[buff.statType];
            stat.value = buff.Apply(stat.value);
        }
        #endregion

        // foreach (var stat in statDic.Values)
        // {
        //     Debug.Log($" {stat.statType} value {stat.value}");
        // }
    }
    public int GetUpgradeLv(StatType type)
    {
        return statDic[type].upgradeLv;
    }
    public static StatType UpgradeTypeToStatType(UpgradeType upgradeType)
    {
        if (Enum.TryParse<StatType>(upgradeType.ToString(), out StatType playerStatType))
        {
            return playerStatType;
        }
        return StatType.Count;
    }
}

[System.Serializable]
public class PlayerStat
{
    public StatType statType;
    public int upgradeLv;
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
    CritChance,      // 추가
    CritPower,  // 추가
    Count
}
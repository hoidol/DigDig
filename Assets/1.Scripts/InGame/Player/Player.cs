using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

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
    public PlayerStatManager statMgr;
    public float angerAmount;
    public Color originColor = new Color(0.1921569f, 1f, 0.4705882f, 1f);
    public Color angerColor = new Color(1f, 0.1921569f, 0.1921569f, 1f); //FF3131 //분노
    [SerializeField] Animator animator;
    public SpriteRenderer[] bodySprites;
    public Transform bodyRootTr;
    public Transform bodyCenterTr;

    public float curHp;

    public int exp;
    public int lv;
    public float attackTimer = 0f;
    public Vector2 LastAttackDir { get; private set; }

    int extraShotCount;
    bool processingExtraShots;

    //여분의 총알 발사 등록하기 - Why? Item, Ability 등 총알이 발사되는 곳이 많을 경우 총알 겹침 방지
    public void QueueExtraShot(int count = 1)
    {
        extraShotCount += count;
        if (!processingExtraShots)
            ProcessExtraShots().Forget();
    }

    async UniTaskVoid ProcessExtraShots()
    {
        processingExtraShots = true;
        var token = this.GetCancellationTokenOnDestroy();
        while (extraShotCount > 0)
        {
            int delay = Mathf.Max(30, 100 - extraShotCount * 10);
            await UniTask.Delay(delay, cancellationToken: token);
            if (token.IsCancellationRequested) return;
            extraShotCount--;
            Attack(GetAttackDirection(), false);
        }
        processingExtraShots = false;
    }

    public int curBulletCount;
    public bool isReloading;
    public float reloadTime = 3f;
    public int gold;
    public List<MagmaBall> magmaBalls = new List<MagmaBall>();
    Camera mainCamara;
    public ItemInventory itemInventory;
    public AbilityInventory abilityInventory;
    public LayerMask pickLayer;
    public Transform hpPoint;

    private void Awake()
    {
        mainCamara = Camera.main;
        moveJoystick = GameObject.Find("MoveJoystick").GetComponent<Joystick>();
        attackJoystack = GameObject.Find("AttackJoystick").GetComponent<Joystick>();

        rg = GetComponentInChildren<Rigidbody2D>();
        magmaBalls = GetComponentsInChildren<MagmaBall>().ToList();
        itemInventory = GetComponentInChildren<ItemInventory>();
        abilityInventory = GetComponentInChildren<AbilityInventory>();

        statMgr = new PlayerStatManager(this, key);
    }

    void Start()
    {
        SetBodyColor(originColor);
        curHp = statMgr.MaxHp;
        curBulletCount = statMgr.BulletCount;
        UpdatePlayer();

        RunRecover().Forget();
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
    public Transform dirTr;
    void Move()
    {
        rg.linearVelocity = moveJoystick.Direction * statMgr.MoveSpeed;

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
        rg.linearVelocity = moveDir * statMgr.MoveSpeed;

#else
        //moveDir = moveJoystick.Direction;
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

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddExp(10);
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            TakeDamage(40);
            abilityInventory.AddAbility("InstantHeal");
        }
#endif
    }
    public Vector2 GetAttackDirection()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = mainCamara.WorldToScreenPoint(attackPoint.position).z;
        Vector3 worldMousePos = mainCamara.ScreenToWorldPoint(mousePosition);
        return (worldMousePos - attackPoint.position).normalized;

    }
    public void UpdateAttack()
    {
        dirTr.up = GetAttackDirection();

        if (isReloading) return;

        attackTimer += Time.deltaTime;

#if UNITY_EDITOR


        if (attackTimer >= statMgr.AttackSpeed)
        {
            // Vector3 mousePosition = Input.mousePosition;
            // mousePosition.z = mainCamara.WorldToScreenPoint(attackPoint.position).z;
            // Vector3 worldMousePos = mainCamara.ScreenToWorldPoint(mousePosition);
            // Vector2 dir = (worldMousePos - attackPoint.position).normalized;


            Attack(GetAttackDirection(), true);
        }
#else
        if (attackJoystack.Direction.magnitude > 0 && attackTimer >= statMgr.AttackSpeed)
        {
            dirTr.up = attackJoystack.Direction;
            Attack(attackJoystack.Direction.normalized, true);
        }
        dirTr.up = attackJoystack.Direction;
#endif
    }
    public void AddGold(int gold)
    {
        this.gold += gold;
        GameEventBus.Publish(new GoldChangedEvent(gold));
    }


    public float healMultiplier = 1f;

    public void AddHp(float hp)
    {
        if (hp > 0) hp *= healMultiplier;
        curHp += hp;
        if (hp > 0)
            HealText.SetText(hpPoint.position, ((int)hp).ToString());

        if (curHp > statMgr.MaxHp)
            curHp = statMgr.MaxHp;

        GameEventBus.Publish(new PlayerHpChangedEvent(curHp, statMgr.MaxHp));
    }
    StatusEffectHandler statusEffectHandler;
    public void TakeDamage(float damage)
    {
        if (statusEffectHandler != null && statusEffectHandler.TryBlock()) return;

        curHp -= damage;
        GameEventBus.Publish(new PlayerHpChangedEvent(curHp, statMgr.MaxHp));
        if (curHp < 0)
        {
            GameManager.Instance.EndGame(false);
        }
    }
    int pendingSpread;
    public void RequestSpread(int count) => pendingSpread += count;


    BulletFiredEvent bulletFiredEvent = new BulletFiredEvent();
    public void Attack(Vector2 dir, bool fromPlayer)
    {
        LastAttackDir = dir;
        pendingSpread = 0;

        // 1단계: 발사 전 준비 (spread 수집)
        foreach (var e in itemInventory.preAttackItems)
            e.OnPreAttack(this, dir);
        foreach (var e in abilityInventory.preAttackItems)
            e.OnPreAttack(this, dir);

        // 2단계: 메인 탄 + 확산 발사
        Shoot(dir, attackPoint.position, fromPlayer);

        foreach (var e in itemInventory.attackItems)
            e.OnAttack(this, dir);
        foreach (var e in abilityInventory.attackItems)
            e.OnAttack(this, dir);

        if (fromPlayer)
            RunComboAttacks(dir).Forget();


        attackTimer = 0f;

        if (fromPlayer)
        {
            curBulletCount--;
            if (curBulletCount <= 0)
                CoReload().Forget();
        }


        bulletFiredEvent.currentBulletCount = curBulletCount;
        bulletFiredEvent.maxBulletCount = statMgr.BulletCount;
        bulletFiredEvent.fromPlayer = fromPlayer;
        bulletFiredEvent.dir = dir;
        GameEventBus.Publish(bulletFiredEvent);

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
        foreach (var e in itemInventory.comboAttackItems)
            await e.OnAttack(this, dir);
        foreach (var e in abilityInventory.comboAttackItems)
            await e.OnAttack(this, dir);
    }


    async UniTaskVoid RunRecover()
    {
        var token = this.GetCancellationTokenOnDestroy();
        while (!token.IsCancellationRequested)
        {
            if (statMgr.RecoveryHp > 0)
                AddHp(statMgr.RecoveryHp);

            await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: token);
        }
    }

    public void Shoot(Vector2 dir, Vector2 pos, bool fromPlayer = false)
    {
        if (pos == Vector2.zero)
            pos = attackPoint.position;

        int totalBullet = fromPlayer ? 1 + pendingSpread : 1;

        if (totalBullet == 1)
        {
            FireBullet(dir, pos);
            return;
        }

        float totalAngle = 30f;
        float angleStep = totalAngle / totalBullet;
        float baseAngle = Vector2.SignedAngle(Vector2.right, dir);

        for (int i = 0; i < totalBullet; i++)
        {
            float angle = baseAngle - totalAngle / 2f + angleStep * 0.5f + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;
            FireBullet(new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)), pos);
        }
    }

    void FireBullet(Vector2 dir, Vector2 pos)
    {
        var bullet = PlayerBullet.Instantiate();
        bullet.ClearBehaviors();
        bullet.ClearBulletForce();
        bullet.transform.position = pos;

        foreach (var e in itemInventory.bulletItems)
            e.OnBulletFired(bullet);
        foreach (var e in abilityInventory.bulletItems)
            e.OnBulletFired(bullet);

        bullet.Shoot(dir, statMgr.AttackPower);
    }

    //public float reloadTimer;
    async UniTaskVoid CoReload()
    {
        isReloading = true;
        GameEventBus.Publish(new ReloadStartEvent(reloadTime, statMgr.ReloadSpeed));
        float maxCount = statMgr.BulletCount;
        float sec = statMgr.ReloadTime / maxCount;
        curBulletCount = 0;
        while (curBulletCount < maxCount)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(sec), cancellationToken: this.GetCancellationTokenOnDestroy());
            curBulletCount++;
            GameEventBus.Publish(new BulletChargedEvent(curBulletCount, (int)maxCount));
        }
        curBulletCount = statMgr.BulletCount;
        isReloading = false;
        attackTimer = statMgr.AttackSpeed;
        GameEventBus.Publish(new ReloadEndEvent());
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

    public void UpdatePlayer()
    {
        #region  Upgrade에 따른 능력치 적용
        float preMaxHp = statMgr.MaxHp;
        statMgr.UpdateStat();
        float curMaxHp = statMgr.MaxHp;
        float diffMaxHp = curMaxHp - preMaxHp;
        if (diffMaxHp > 0)
        {
            AddHp(diffMaxHp);
        }

        #endregion

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

[System.Serializable]
public class PlayerStatManager
{
    [SerializeField] List<PlayerStat> statList = new(); // 인스펙터 확인용 (statDic과 동일 객체 공유)
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
    StatUpAbilityData[] statUpAbilityDatas;
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
        statUpAbilityDatas = Resources.LoadAll<StatUpAbilityData>("AbilityData/StatUpAbilityData");
        Init();
    }
    void Init()
    {
        for (int i = 0; i < (int)StatType.Count; i++)
        {
            StatType statType = (StatType)i;
            // Debug.Log($"StatType {statType}");
            statDic[statType].value = playerData.GetPlayerStat(statType).value;
        }
    }

    public void UpdateStat()
    {
        Init();
        #region  Ability 따른 능력치 적용
        for (int i = 0; i < statUpAbilityDatas.Length; i++)
        {
            StatUpAbility pAbility = player.abilityInventory.GetAbility(statUpAbilityDatas[i].key) as StatUpAbility;
            if (pAbility == null || pAbility.count <= 0)
                continue;
            var stat = statDic[pAbility.statType];
            stat.value = pAbility.Apply(stat.value, pAbility.count);

            Debug.Log($"Ability 따른 능력치 적용 {pAbility.key} value {stat.value}");
        }

        #endregion

        #region Buff 적용
        foreach (var buff in activeBuffs)
        {
            var stat = statDic[buff.statType];
            stat.value = buff.Apply(stat.value);
        }
        #endregion
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
    public bool fromPlayer
    {
        get; set;
    }
    public int currentBulletCount
    {
        get;
        set;
    }
    public int maxBulletCount
    {
        get;
        set;
    }
    public Vector2 dir;
}

public class ReloadEvent
{

}

public class PlayerUpdateEvent
{
    public Player player;
    public PlayerUpdateEvent(Player player)
    {
        this.player = player;
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;

public class Player : MonoSingleton<Player>, IPicker, IHittable
{
    public const int COMBO_ATTACK_INTERVAL_MS = 70;
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
    public CameraShake cameraShake;
    public int maxDistance; //플레이어가 중심점에서 멀어진 최대 길이
    //Vector2 centerPoint;
    float maxDistanceSqr;

    public float curHp;

    public int exp;
    public int lv;
    public float attackTimer = 0f;
    public Vector2 LastAttackDir { get; private set; }

    public int extraShotCount;
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
        statusEffectHandler = GetComponentInChildren<StatusEffectHandler>();

        statMgr = new PlayerStatManager(this, key);
    }

    void Start()
    {
        GameEventBus.Subscribe<UndergroundStartEvent>(OnUndergroundStart);

        curHp = statMgr.MaxHp;
        UpdatePlayer();
        RunRecover().Forget();
    }

    void OnUndergroundStart(UndergroundStartEvent e)
    {
        //centerPoint = transform.position;
        maxDistance = 0;
        maxDistanceSqr = 0f;

        SetBodyColor(originColor);

        isReloading = false;
        isAnger = false;
        angerAmount = 0;
        attackTimer = statMgr.AttackSpeed;
        extraShotCount = 0;
        processingExtraShots = false;
        pendingMultiShot = 1;
        pendingSpread = 0;
        curBulletCount = statMgr.BulletCount;

        StopAllCoroutines();
        GameEventBus.Publish(new ReloadEndEvent());
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
    public Vector2 MoveDirection { get; private set; }

    void Move()
    {
#if UNITY_EDITOR||!UNITY_ANDROID && !UNITY_IOS
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        MoveDirection = new Vector2(x, y).normalized;
#else
        MoveDirection = moveJoystick.Direction;
#endif
        if (MoveDirection.magnitude > 0.1f)
        {
            bodyRootTr.localScale = new Vector3(MoveDirection.x >= 0 ? 1 : -1, 1, 1);
            animator.SetBool("Running", true);

            float sqrDist = ((Vector2)transform.position - Vector2.zero).sqrMagnitude;
            if (sqrDist > maxDistanceSqr)
            {
                maxDistanceSqr = sqrDist;
                maxDistance = (int)Mathf.Sqrt(sqrDist);
            }
        }
        else
        {
            animator.SetBool("Running", false);
        }

        rg.linearVelocity = MoveDirection * statMgr.MoveSpeed;
    }
    void Update()
    {
        if (GameManager.Instance.isClear)
            return;
        Move();
        UpdateAttack();

        angerAmount -= Time.deltaTime;
        if (angerAmount < 0)
            angerAmount = 0;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.L))
        {
            AddExp(10);
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            TakeDamage(new DamageData { damage = 40 });
            //abilityInventory.AddAbility("InstantHeal");
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
        GameEventBus.Publish(new GoldChangedEvent(this.gold, gold));
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
    int pendingMultiShot; // 기본이 1야
    int pendingSpread; //기본이 0이야. 만약이 1이되면 오른쪽부터 15도 2면 왼쪽 15도 추가 3면 오른쪽 30도 추가 이런씩으로 증가
    public void RequestMulti(int count) => pendingMultiShot += count;
    public void RequestSpread(int count) => pendingSpread += count;

    BulletFiredEvent bulletFiredEvent = new BulletFiredEvent();
    bool levelUped;
    [SerializeField] int maxExp;

    public void Attack(Vector2 dir, bool fromPlayer)
    {
        LastAttackDir = dir;
        pendingMultiShot = 1;
        pendingSpread = 0;

        // 1단계: 발사 전 준비 (spread 수집)
        foreach (var e in itemInventory.preAttackItems)
            e.OnPreAttack(this, dir);
        foreach (var e in abilityInventory.preAttackItems)
            e.OnPreAttack(this, dir);

        // 2단계: multiShot 횟수만큼 나란히 발사 (수직 오프셋)
        Vector2 perp = new(-dir.y, dir.x);
        const float MULTI_SPACING = 0.2f;
        float startOffset = -(pendingMultiShot - 1) * 0.5f * MULTI_SPACING;
        for (int i = 0; i < pendingMultiShot; i++)
        {
            Vector2 pos = (Vector2)attackPoint.position + perp * (startOffset + MULTI_SPACING * i);
            Shoot(dir, pos);
        }

        // i=0 → 오른쪽 15°, i=1 → 왼쪽 15°, i=2 → 오른쪽 30°, i=3 → 왼쪽 30°...
        float baseAngle = Vector2.SignedAngle(Vector2.right, dir);
        for (int i = 0; i < pendingSpread; i++)
        {
            int sign = (i % 2 == 0) ? 1 : -1;
            float offset = (i / 2 + 1) * 40f;
            float rad = (baseAngle + sign * offset) * Mathf.Deg2Rad;
            Shoot(new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)), attackPoint.position);
        }

        foreach (var e in itemInventory.attackItems)
            e.OnAttack(this, dir);
        foreach (var e in abilityInventory.attackItems)
            e.OnAttack(this, dir);

        if (fromPlayer)
        {
            RunComboAttacks(dir).Forget();
            cameraShake.Shake(0.15f);
        }

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

        ;

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

    public PlayerBullet Shoot(Vector2 dir, Vector2 pos)
    {
        if (pos == Vector2.zero)
            pos = attackPoint.position;

        var bullet = PlayerBullet.Instantiate();
        bullet.ClearBehaviors();
        bullet.ClearBulletForce();
        bullet.transform.position = pos;

        foreach (var e in itemInventory.bulletItems)
            e.OnBulletFired(bullet);
        foreach (var e in abilityInventory.bulletItems)
            e.OnBulletFired(bullet);

        bullet.Shoot(dir, statMgr.AttackPower);
        return bullet;
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
            // Debug.Log("LevelUp!");
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
    void LevelUp()
    {
        int remain = exp - GetMaxExp();
        exp = remain;
        lv++;
        levelUped = true;
        //강화 UI 활성화
    }

    public int GetMaxExp(int l = -1)
    {
        if (l == -1)
            l = lv;

        if (maxExp == 0 || levelUped)
        {
            int increaseValue = 3;

            maxExp = 5 + l * increaseValue;
            levelUped = false;
        }

        return maxExp;
    }

    public void TakeDamage(DamageData damageData)
    {
        if (statusEffectHandler != null && statusEffectHandler.TryBlock()) return;

        PlayerTakeDamageText.SetText(hpPoint.position, $"-{((int)damageData.damage).ToString()}");
        curHp -= damageData.damage;
        GameEventBus.Publish(new PlayerHpChangedEvent(curHp, statMgr.MaxHp));
        if (curHp < 0)
        {
            GameManager.Instance.EndGame(false);
        }
    }

    public bool CanHit()
    {
        return curHp > 0;
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
            // Debug.Log($"Ability 따른 능력치 적용 {pAbility.key} value {stat.value}");
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


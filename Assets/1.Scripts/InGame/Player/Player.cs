using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.Pool;
using Unity.Burst.Intrinsics;
using System;

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
    // public Transform faceTr;
    //public ParticleSystem pickParticle;
    public PlayerStatManager playerStatMgr;
    public float angerAmount;
    public Color originColor = new Color(0.1921569f, 1f, 0.4705882f, 1f);
    public Color angerColor = new Color(1f, 0.1921569f, 0.1921569f, 1f); //FF3131 //분노
    [SerializeField] Animator animator;
    public SpriteRenderer[] bodySprites;
    public Transform bodyRootTr;

    // public float maxHp;
    public float curHp;

    public int exp;
    public int lv;
    public float attackTimer = 0f;
    public int gold;
    public List<MagmaBall> magmaBalls = new List<MagmaBall>();
    //public MagmaRotation magmaRotation;

    public Inventory inventory;
    public List<PlayerAbility> playerAbilities = new List<PlayerAbility>();


    // public float pickRange = 2f;
    public LayerMask pickLayer;


    public void OnDrawGizmos()
    {
        if (playerStatMgr == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerStatMgr.AttackPower);
    }

    private void Awake()
    {
        joystick = FindFirstObjectByType<Joystick>();
        rg = GetComponentInChildren<Rigidbody2D>();
        magmaBalls = GetComponentsInChildren<MagmaBall>().ToList();
        //magmaRotation = GetComponentInChildren<MagmaRotation>();
        inventory = GetComponentInChildren<Inventory>();
        playerStatMgr = new PlayerStatManager(this, key);
    }

    void Start()
    {
        SetBodyColor(originColor);
        playerStatMgr.UpdateStat();
        curHp = playerStatMgr.MaxHp;
        //StartCoroutine(CoBodyEffect());
    }
    void SetBodyColor(Color color)
    {
        for (int i = 0; i < bodySprites.Length; i++)
        {
            bodySprites[i].color = color;
        }
    }
    bool isAnger;
    void Update()
    {
        rg.linearVelocity = joystick.Direction * playerStatMgr.MoveSpeed;

        if (joystick.Direction.magnitude > 0.1f)
        {
            if (joystick.Direction.x >= 0)
                bodyRootTr.localScale = new Vector3(1, 1, 1);
            else
                bodyRootTr.localScale = new Vector3(-1, 1, 1);
            animator.SetBool("Running", true);
        }
        else
        {
            animator.SetBool("Running", false);
        }

        attackTimer += Time.deltaTime;
        if (attackTimer >= playerStatMgr.AttackSpeed)
        {
            attackTimer = playerStatMgr.AttackSpeed;
            Attack();
        }

        angerAmount -= Time.deltaTime;
        if (angerAmount < 0)
            angerAmount = 0;

        // if (!isAnger)
        // {
        //     magmaRotation.SetRotationSpeed(-150);
        // }
        // else
        // {
        //     magmaRotation.SetRotationSpeed(-450);
        // }


        magmaBalls.ForEach(ball => ball.SetColor(bodySprites[0].color));
    }


    public void AddGold(int gold)
    {
        this.gold += gold;
    }
    IEnumerator CoBodyEffect()
    {
        while (true)
        {
            if (isAnger)
            {
                yield return bodyRootTr.DOScale(0.9f, 0.5f).SetEase(Ease.OutBack).WaitForCompletion();
                yield return bodyRootTr.DOScale(1f, 0.5f).SetEase(Ease.InBack).WaitForCompletion();
            }
            else
            {
                yield return bodyRootTr.DOScale(0.99f, 1.5f).SetEase(Ease.OutBack).WaitForCompletion();
                yield return bodyRootTr.DOScale(1f, 1.5f).SetEase(Ease.InBack).WaitForCompletion();
            }
        }
    }

    void Attack()
    {
        //마지막 방향으로
        //lastVec 방향으로 쏘기
        TargetIndicator.Instance.SetTaret(null);



        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, playerStatMgr.AttackRange, pickLayer);
        Collider2D closestCollider = null;
        float closestDistance = float.MaxValue;

        var enemyColliders = colliders
            .Where(c => c.CompareTag("Enemy"))
            // Vector2.Distance(a, b) 대신 제곱거리((a-b).sqrMagnitude)로 대체하여 연산을 가볍게 함
            .OrderBy(c => (c.transform.position - transform.position).sqrMagnitude)
            .ToArray();

        Collider2D nearestEnemyCollider = enemyColliders.FirstOrDefault();
        if (nearestEnemyCollider != null && nearestEnemyCollider.TryGetComponent(out IHittable enemyHittable))
        {
            SetTarget(enemyHittable);
            return;
        }

        if (target != null && target.Transform != null && target.Transform.gameObject.activeSelf)
        {
            float distance = Vector2.Distance(transform.position, target.Transform.position);
            if (distance <= playerStatMgr.AttackRange)
            {
                SetTarget(target);
                return;
            }

        }

        foreach (var collider in colliders)
        {
            float distance = Vector2.Distance(transform.position, collider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCollider = collider;
            }
        }

        if (closestCollider != null && closestCollider.TryGetComponent(out IHittable hittable))
        {
            SetTarget(hittable);
        }
    }
    public void AddHp(float hp)
    {
        curHp += hp;
        if (curHp > playerStatMgr.MaxHp)
        {
            curHp = playerStatMgr.MaxHp;
        }
    }

    public void TakeDamage(float damage)
    {
        curHp -= damage;
        if (curHp < 0)
        {
            GameManager.Instance.EndGame(false);
        }
    }
    IHittable target;
    void SetTarget(IHittable t)
    {
        target = t;
        TargetIndicator.Instance.SetTaret(target.Transform);
        Attack(target.Transform.position - transform.position);
    }
    void Attack(Vector2 dir)//IHittable hittable
    {

        FlameBullet flameBullet = FlameBullet.Instantiate();
        flameBullet.transform.position = attackPoint.position;
        flameBullet.Shoot(dir, playerStatMgr.AttackPower);

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
            });
        }
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
            maxExp = 10 + lv * 5;
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
    public float MaxHp => statDic[StatType.MaxHp].value;
    public float AttackPower => statDic[StatType.AttackPower].value;
    public float MoveSpeed => statDic[StatType.MoveSpeed].value;
    public float RecoveryHp => statDic[StatType.RecoveryHp].value;
    public float AttackSpeed => statDic[StatType.AttackSpeed].value;
    public float AttackRange => statDic[StatType.AttackRange].value;

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
    Count
}
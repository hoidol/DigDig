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
    public string key;
    public Rigidbody2D rg;
    public Joystick joystick;
    public Transform faceTr;
    public ParticleSystem pickParticle;
    public PlayerStatManager playerStatMgr;
    public float angerAmount;
    public Color originColor = new Color(0.1921569f, 1f, 0.4705882f, 1f);
    public Color angerColor = new Color(1f, 0.1921569f, 0.1921569f, 1f); //FF3131 //분노
    //public Color[] angerStepColors;
    // public int angerStepIndex = 0;
    // public AngerStep[] angerSteps;
    public SpriteRenderer bodySprite;
    public Transform bodyTr;

    public float maxHp;
    public float curHp;

    public int exp;
    public int lv;
    public float attackTimer = 0f;
    public int gold;
    public List<MagmaBall> magmaBalls = new List<MagmaBall>();
    public MagmaRotation magmaRotation;

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
        magmaRotation = GetComponentInChildren<MagmaRotation>();
        inventory = GetComponentInChildren<Inventory>();
        playerStatMgr = new PlayerStatManager(key);
    }

    void Start()
    {
        bodySprite.color = originColor;
        playerStatMgr.UpdateStat();
        StartCoroutine(CoBodyEffect());
    }
    bool isAnger;
    void Update()
    {
        rg.linearVelocity = joystick.Direction * playerStatMgr.MoveSpeed;

        if (joystick.Direction.magnitude > 0.1f)
        {
            if (joystick.Direction.x >= 0)
                faceTr.localScale = new Vector3(1, 1, 1);
            else
                faceTr.localScale = new Vector3(-1, 1, 1);

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

        if (!isAnger)
        {
            magmaRotation.SetRotationSpeed(-150);
        }
        else
        {
            magmaRotation.SetRotationSpeed(-450);
        }


        magmaBalls.ForEach(ball => ball.SetColor(bodySprite.color));
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
                yield return bodyTr.DOScale(0.9f, 0.5f).SetEase(Ease.OutBack).WaitForCompletion();
                yield return bodyTr.DOScale(1f, 0.5f).SetEase(Ease.InBack).WaitForCompletion();
            }
            else
            {
                yield return bodyTr.DOScale(0.99f, 1.5f).SetEase(Ease.OutBack).WaitForCompletion();
                yield return bodyTr.DOScale(1f, 1.5f).SetEase(Ease.InBack).WaitForCompletion();
            }
        }
    }

    void Attack()
    {
        //마지막 방향으로
        //lastVec 방향으로 쏘기


        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, playerStatMgr.AttackRange, pickLayer);
        Collider2D closestCollider = null;
        float closestDistance = float.MaxValue;
        // colliders 배열에서 Enemy 태그를 가진 것 중 가장 가까운 콜라이더만 대상으로 판별
        // System.Linq을 활용하여 최적화된 코드
        var enemyColliders = colliders
            .Where(c => c.CompareTag("Enemy"))
            // Vector2.Distance(a, b) 대신 제곱거리((a-b).sqrMagnitude)로 대체하여 연산을 가볍게 함
            .OrderBy(c => (c.transform.position - transform.position).sqrMagnitude)
            .ToArray();

        Collider2D nearestEnemyCollider = enemyColliders.FirstOrDefault();
        if (nearestEnemyCollider != null && nearestEnemyCollider.TryGetComponent(out IHittable enemyHittable))
        {
            Attack(enemyHittable.Transform.position - transform.position);
            return;
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
            Attack(hittable.Transform.position - transform.position);
        }
    }
    public void AddHp(float hp)
    {
        curHp += hp;
        if (curHp > maxHp)
        {
            curHp = maxHp;
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
    void Attack(Vector2 dir)//IHittable hittable
    {

        FlameBullet flameBullet = FlameBullet.Instantiate();
        flameBullet.transform.position = transform.position;
        flameBullet.Shoot(dir, playerStatMgr.AttackPower);

        attackTimer = 0f;

        if (dir.x >= 0)
            faceTr.localScale = new Vector3(1, 1, 1);
        else
            faceTr.localScale = new Vector3(-1, 1, 1);

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
        bodySprite.color = angerColor;
        yield return new WaitForSeconds(10f);
        isAnger = false;
        angerAmount = 0;
        bodySprite.color = originColor;
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
        float preMaxHp = playerStatMgr.MaxHp;
        playerStatMgr.UpdateStat();
        float curMaxHp = playerStatMgr.MaxHp;
        float diffMaxHp = curMaxHp - preMaxHp;
        if (diffMaxHp > 0)
        {
            AddHp(diffMaxHp);
        }

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
    public Dictionary<PlayerStatType, PlayerStat> playerStatDic = new Dictionary<PlayerStatType, PlayerStat>();
    public float MaxHp => playerStatDic[PlayerStatType.MaxHp].value;
    public float AttackPower => playerStatDic[PlayerStatType.AttackPower].value;
    public float MoveSpeed => playerStatDic[PlayerStatType.MoveSpeed].value;
    public float RecoveryHp => playerStatDic[PlayerStatType.RecoveryHp].value;
    public float AttackSpeed => playerStatDic[PlayerStatType.AttackSpeed].value;
    public float AttackRange => playerStatDic[PlayerStatType.AttackRange].value;

    PlayerData playerData;
    public PlayerStatManager(string key)
    {
        playerData = Resources.Load<PlayerData>($"PlayerData/{key}");
        PlayerStat[] playerStats = new PlayerStat[(int)PlayerStatType.Count];
        for (int i = 0; i < (int)PlayerStatType.Count; i++)
        {
            playerStats[i] = new PlayerStat();
            playerStats[i].playerStatType = (PlayerStatType)i;
            playerStatDic.Add(playerStats[i].playerStatType, playerStats[i]);
        }

        Init();
    }
    void Init()
    {
        playerStatDic[PlayerStatType.MaxHp].value = playerData.GetPlayerStat(PlayerStatType.MaxHp).value;
        playerStatDic[PlayerStatType.AttackPower].value = playerData.GetPlayerStat(PlayerStatType.AttackPower).value;
        playerStatDic[PlayerStatType.MoveSpeed].value = playerData.GetPlayerStat(PlayerStatType.MoveSpeed).value;
        playerStatDic[PlayerStatType.AttackSpeed].value = playerData.GetPlayerStat(PlayerStatType.AttackSpeed).value;
        playerStatDic[PlayerStatType.AttackRange].value = playerData.GetPlayerStat(PlayerStatType.AttackRange).value;
        playerStatDic[PlayerStatType.RecoveryHp].value = playerData.GetPlayerStat(PlayerStatType.RecoveryHp).value;
    }
    public void Upgrade(UpgradeType upgradeType)
    {
        playerStatDic[UpgradeTypeToPlayerStatType(upgradeType)].upgradeLv++;
        UpdateStat();
    }
    public void UpdateStat()
    {
        Init();

        playerStatDic[PlayerStatType.AttackPower].value = AttackPower + UpgradeData.GetUpgradeData(UpgradeType.AttackPower).GetValue(playerStatDic[PlayerStatType.AttackPower].upgradeLv);
        playerStatDic[PlayerStatType.MaxHp].value = MaxHp + UpgradeData.GetUpgradeData(UpgradeType.MaxHp).GetValue(playerStatDic[PlayerStatType.MaxHp].upgradeLv);
        playerStatDic[PlayerStatType.RecoveryHp].value = RecoveryHp + UpgradeData.GetUpgradeData(UpgradeType.RecoveryHp).GetValue(playerStatDic[PlayerStatType.RecoveryHp].upgradeLv);
    }
    public int GetUpgradeLv(PlayerStatType type)
    {
        return playerStatDic[type].upgradeLv;
    }
    public static PlayerStatType UpgradeTypeToPlayerStatType(UpgradeType upgradeType)
    {
        if (Enum.TryParse<PlayerStatType>(upgradeType.ToString(), out PlayerStatType playerStatType))
        {
            return playerStatType;
        }
        return PlayerStatType.Count;
    }
}

[System.Serializable]
public class PlayerStat
{
    public PlayerStatType playerStatType;
    public int upgradeLv;
    public float value;
}


public enum PlayerStatType
{
    AttackPower,
    MaxHp,
    RecoveryHp,
    AttackSpeed,
    MoveSpeed,
    AttackRange,
    Count
}
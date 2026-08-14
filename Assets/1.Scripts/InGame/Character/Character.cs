using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Character : MonoSingleton<Character>, IPicker
{
    public const int COMBO_ATTACK_INTERVAL_MS = 70;

    public CharacterName characterName;
    public Rigidbody2D rg;
    public Joystick moveJoystick;
    public Joystick attackJoystick;
    public CharacterStatManager statMgr;
    public Animator animator;
    public Transform bodyRootTr;
    public Transform bodyCenterTr;
    public CameraShake cameraShake;

    // public int exp;
    // public int lv;

    // bool levelUped;
    // [SerializeField] int maxExp;
    // public int bounce;
    [SerializeField] Transform hpPoint;
    public ItemInventory itemInventory; //패시브 스킬로 제공!
    // public OreInventory oreInventory; //패시브 스킬로 제공!
    public TileChecker[] tileCheckers;
    public CharacterHealth health;
    public CharacterMovement movement;
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
    public Vector2 AttackDir => weapon.GetAttackDirection();
    

    public Transform Transform => transform;
    public int orePiece;

    private void Awake()
    {
        moveJoystick = GameObject.Find("MoveJoystick").GetComponent<Joystick>();
        attackJoystick = GameObject.Find("AttackJoystick").GetComponent<Joystick>();

        rg = GetComponentInChildren<Rigidbody2D>();
        itemInventory = GetComponentInChildren<ItemInventory>();
        // abilityInventory = GetComponentInChildren<AbilityInventory>();
        // statInventory = GetComponentInChildren<StatInventory>();
        health = GetComponentInChildren<CharacterHealth>();
        movement = GetComponentInChildren<CharacterMovement>();
        weapon = GetComponentInChildren<BaseGun>();


        tileCheckers = GetComponentsInChildren<TileChecker>();
        // subMachines = GetComponentsInChildren<SubMachine>();
    }


    void Start()
    {
        statMgr = new CharacterStatManager(this, characterName);

        var statusEffectHandler = GetComponentInChildren<StatusEffectHandler>();
        health.Init(this, hpPoint, statusEffectHandler);
        movement.Init(this, rg, animator, bodyRootTr);
        weapon.Init(this);

        UpdateCharacter();

        // bounce = 0;
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
        // if (!GameManager.Instance.isPlaying) return;

        movement.Move();
        weapon.UpdateWeapon();

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
        GameEventBus.Publish(new CharacterUpdateEvent(this));
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
    //         LevelUpCanvas.Instance.OpenCanvas(() =>
    //         {
    //             Time.timeScale = 1;
    //             AddExp(0);
    //         });
    //     }
    //     GameEventBus.Publish(new ExpChangedEvent(exp, GetMaxExp()));
    // }

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
    //         maxExp = 5 + l * 3;
    //         levelUped = false;
    //     }
    //     return maxExp;
    // }



    public void AddItem(string key, int count = 1)
    {
        statMgr.AddItem(key, count);
        itemInventory.AddItem(key);
        UpdateCharacter();
    }
    public void RemoveItem(string key, int idx =-1)
    {
        statMgr.AddItem(key,-1);

        itemInventory.RemoveItem(key, idx);
        itemInventory.UpdateInventory();
        UpdateCharacter();
    }
    public void AddOrePiece(int count)
    {
        orePiece += count;
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
    public CharacterBulletObject Shoot(Bullet b, Vector2 dir) => weapon.Shoot(b, dir);

    // public void QueueExtraShot(int count = 1) => weapon.QueueExtraShot(count);

}

//공격력(+ float), 체력(+ float), 치명타 확률(+ float), 치명타 피해량(+ float), 바운스(+ int), 이동속도(+ float 1초동안 얼만큼 가는지), 공격속도(+ float 1초동안 몇발 쏘는지)
public enum StatType
{
    AttackPower, //float
    MaxHp, //float
    RecoveryHp, //float 초당 얼마나 회복될지
    AttackSpeed, //float 10초동안 몇발 쏘는지
    MoveSpeed, //float 10초동안 얼만큼 가는지
    CritChance, //float
    CritPower, //float
    Dodge, //float 
    // AmmoEfficiency, // 튕기는 때 데미지 감소량을 줄어듦 - 버프 주지마
    // Bounce, //튕기는 횟수
    Count
}

public class BulletFiredEvent
{
    public Bullet bullet;
    public Vector2 dir;
}


public class CharacterUpdateEvent
{
    public Character character;
    public CharacterUpdateEvent(Character character) { this.character = character; }
}

using UnityEngine.Serialization;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;

public class Slime : MonoSingleton<Slime>, IPicker, IAllyUnit, IHittable, ITileObject, IBuffable, IShotModifiable
{
    public const int COMBO_ATTACK_INTERVAL_MS = 70;

    public SlimeName slimeName;
    public Rigidbody2D rg;
    // public Joystick moveJoystick;
    // public Joystick attackJoystick;
    public SlimeStatManager statMgr;
    public SlimeAbilityInventory abilityInventory; // (2026-11-20) 레벨업 카드로 얻은 슬라임 성장 능력 (한 판 동안)
    public Animator animator;
    public Transform bodyRootTr;
    public Transform bodyCenterTr;
    public CameraShake cameraShake;
    public float coinChance = 0.5f;

    public FindTargetType findTargetType = FindTargetType.First; // (2026-11-24) 기본 = 가장 먼저 나온 적 (Start에서 씬 값도 덮어씀)

    public FindTargetType FindTargetType => findTargetType;
    // (2026-11-25) 실제로 쓰는 타겟 방식 (용사 스킬이 잠깐 바꿀 수 있음, 레인저 매의 눈)
    public FindTargetType EffectiveFindTargetType => findTargetType != FindTargetType.Random && HeroSlimeAbility.ForcedAllyFindTarget.HasValue
        ? HeroSlimeAbility.ForcedAllyFindTarget.Value : findTargetType;

    // (2026-11-20) 인게임 슬라임 레벨 (한 판 동안). 레벨업 창은 아직 안 띄움, 상단 경험치바(ExpBar)만
    public int exp;
    public int lv;
    public int MaxExp => GetMaxExp(lv);
    // 보너스 엘리트 처치로 모아 둔 다음 브레이크타임 주사위 코인 (MinionOfferSystem이 받아 감)
    public int bonusDiceCoinNextBreak;
    // (2026-11-23) 엘리트·보스 처치로 모아 둔 룬 보상 수 → 다음 브레이크타임에 룬 선택 창 (RuneRewardSystem)
    public int pendingRuneRewards;

    [SerializeField] Transform hpPoint;
    [FormerlySerializedAs("itemInventory")] public RuneInventory runeInventory; //패시브 스킬로 제공!
    public MinionInventory minionInventory;
    // public OreInventory oreInventory; //패시브 스킬로 제공!
    public TileChecker[] tileCheckers;
    public SlimeHealth health;
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
    // (2026-11-21) 슬라임 공격 타입 = 무기 설정 한 곳 (화면 표시·탄·패시브 폭발 공통). 이전: 물리 고정이라 화면(카오스)과 실제(물리)가 달랐음
    public AttackType AttackType => weapon != null ? weapon.attackType : AttackType.Chaos;

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
        runeInventory = GetComponentInChildren<RuneInventory>();
        minionInventory = GetComponentInChildren<MinionInventory>();
        // statInventory = GetComponentInChildren<StatInventory>();
        health = GetComponentInChildren<SlimeHealth>();
        // movement = GetComponentInChildren<CharacterMovement>();
        weapon = GetComponentInChildren<Weapon>();


        tileCheckers = GetComponentsInChildren<TileChecker>();
        // subMachines = GetComponentsInChildren<SubMachine>();
    }


    void Start()
    {
        findTargetType = FindTargetType.First; // (2026-11-24) 기본 타겟 = 가장 먼저 나온 적 (씬에 저장된 값보다 우선)
        // rangeObj.SetActive(false);
        statMgr = new SlimeStatManager(this, slimeName);
        abilityInventory = new SlimeAbilityInventory(this);

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
    // (2026-11-24) 용사를 고르면 슬라임 기본 스탯 = HeroData.csv의 그 용사 (null = SlimeData로 되돌림)
    // (2026-11-25) heroLevel = 용사 레벨 (레벨업마다 HeroData의 Add* 만큼 오름)
    public void ApplyHeroBaseStats(HeroDataTable.HeroData hero, int heroLevel = 1)
    {
        statMgr.SetHeroBase(hero, heroLevel);
        UpdateCharacter();
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
        else if (diffMaxHp < 0)
            AddHp(0, false); // 최대 체력이 줄면 현재 체력을 최대 체력 이하로 (SlimeHealth.AddHp가 자름)
        // rangeObj.transform.localScale = Vector3.one * statMgr.AttackRange;
        GameEventBus.Publish(new SlimeUpdateEvent(this));
    }

    // 받은 버프의 이펙트 (Buff.effectType, 십자 버프 미니언 등)
    readonly BuffEffectDisplay buffEffectDisplay = new BuffEffectDisplay();

    public void AddBuff(Buff buff)
    {
        statMgr.activeBuffs.Add(buff);
        UpdateCharacter();
        buffEffectDisplay.Refresh(transform, statMgr.activeBuffs);
    }

    public void RemoveBuff(Buff buff)
    {
        statMgr.activeBuffs.Remove(buff);
        UpdateCharacter();
        buffEffectDisplay.Refresh(transform, statMgr.activeBuffs);
    }

    // (2026-11-23) 필요 경험치 = GameSetting.SLIME_LEVEL_EXPS[레벨] (2,2,2,3,3,3,4,4,4). 최대 레벨 이상이면 마지막 값(표시용)
    public static int GetMaxExp(int level)
    {
        int[] table = GameSetting.SLIME_LEVEL_EXPS;
        return table[Mathf.Clamp(level, 0, table.Length - 1)];
    }
    public static bool IsMaxLevel(int level) => level >= GameSetting.SLIME_MAX_LEVEL;
    public bool IsMaxLv => IsMaxLevel(lv);

    // (2026-11-22) 경험치 구매: 주사위 코인 BUY_EXP_DICE_COIN_PRICE개 → 경험치 BUY_EXP_AMOUNT. 코인이 모자라면 false
    public bool TryBuyExp()
    {
        if (IsMaxLv || diceCoin < GameSetting.BUY_EXP_DICE_COIN_PRICE)
            return false;
        AddDiceCoin(-GameSetting.BUY_EXP_DICE_COIN_PRICE);
        // (2026-11-24) 용사 강화 ExpBuyBonusChance: 확률로 경험치 +1 더
        int bonus = HeroUpgradeSystem.RollExpBuyBonus();
        if (bonus > 0)
            EffectManager.PlayPopText((Vector2)transform.position + Vector2.up, $"EXP +{GameSetting.BUY_EXP_AMOUNT + bonus}!", new Color(0.6f, 1f, 0.6f));
        AddExp(GameSetting.BUY_EXP_AMOUNT + bonus);
        return true;
    }

    // 경험치 추가 → 넘치면 레벨업(남은 경험치 이월, 여러 번 가능)
    // (2026-11-22) 경험치가 다 차면 바로 레벨업 창(LevelUpCanvas.OpenPending). 여러 레벨이면 밀린 수(pendingLevelUps)만큼 연속으로 열림
    // 경험치는 브레이크타임에 구매로만 얻으므로 창이 열릴 수 있음 (열 수 없으면 밀어 두고 다음 레벨업 때 함께)
    public void AddExp(int e)
    {
        if (e <= 0 || IsMaxLv)
            return;
        exp += e;
        int levelUps = 0;
        while (!IsMaxLv && exp >= MaxExp)
        {
            exp -= MaxExp;
            lv++;
            pendingLevelUps++;
            levelUps++;
        }
        if (IsMaxLv)
            exp = 0; // 최대 레벨: 남은 경험치는 버림 (MAX 표시)
        GameEventBus.Publish(new ExpChangedEvent(exp, MaxExp, lv));
        // (2026-11-23) 레벨업 보상: 레벨마다 주사위 코인 LEVEL_UP_DICE_COIN (레벨업 창 아래 "레벨업 보상 +1")
        if (levelUps > 0)
            AddDiceCoin(GameSetting.LEVEL_UP_DICE_COIN * levelUps);
        if (pendingLevelUps > 0)
            InGame.LevelUpCanvas.OpenPending();
    }

    // 아직 카드를 고르지 않은 레벨업 수
    public int pendingLevelUps;
    // 다음 레벨업 창의 레벨 (밀린 레벨업은 낮은 레벨부터: Lv.4·5가 밀렸으면 Lv.4 창 → Lv.5 창)
    public int ChoiceLevel => lv - pendingLevelUps + 1;

    // 레벨업 창 하나를 처리함 (카드를 고르거나 고를 카드가 없을 때)
    public void ConsumeLevelUp()
    {
        if (pendingLevelUps <= 0)
            return;
        pendingLevelUps--;
        abilityInventory?.Get<HeroSlimeAbility>()?.RefreshHeroLevel(); // (2026-11-25) 용사 스탯·정예병 칸 버프 상승
        GameEventBus.Publish(new ExpChangedEvent(exp, MaxExp, lv));
    }
    public void SetFindTargetType(FindTargetType type)
    {
        findTargetType = type;
    }




    // 인벤토리가 가득 찼으면 교체 UI를 띄우고 결과를 기다림 (교체 시 ChangeItemCanvas에서 아이템 추가까지 처리)
    public async UniTask<bool> AddRune(string key, Tile tile = null)
    {
        statMgr.AddRune(key, 1);
        runeInventory.AddRune(key, tile);
        runeInventory.UpdateInventory();
        UpdateCharacter();
        return true;
    }

    // 타일 아이템 객체 하나 제거 (같은 key의 다른 타일 객체는 유지)
    public void RemoveRune(TileRune tileRune)
    {
        runeInventory.RemoveRune(tileRune); // curItems에서 먼저 빼야 UpdateInventory가 다른 객체를 건드리지 않음
        statMgr.AddRune(tileRune.key, -1);
        runeInventory.UpdateInventory();
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
        if (count > 0)
            SoundManager.Play(SFXType.DiceCoinGet);
        GameEventBus.Publish(new DiceCoinEvent(diceCoin));
    }

    // (2026-11-25) 보상 주사위 코인(판매 환불·미션·보너스 엘리트 등): 이번 브레이크타임 지급이 끝난 뒤면 즉시, 아니면 다음 브레이크타임에
    // 반환 = 즉시 받았는지 (글씨 "+N" / "다음 웨이브 +N" 구분용)
    public bool AddRewardDiceCoin(int count)
    {
        if (count <= 0)
            return false;
        if (MinionOfferSystem.IsBreakDiceGranted)
        {
            AddDiceCoin(count);
            return true;
        }
        bonusDiceCoinNextBreak += count;
        return false;
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
    public SlimeBulletObject Shoot(BulletSpec b, Vector2 dir) => weapon.Shoot(b, dir);

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

    // (2026-11-21) 드래그 중엔 슬라임 공격 멈춤 (Weapon.UpdateWeapon)
    public bool IsBeingDragged { get; private set; }

    public void StartDrag()
    {
        IsBeingDragged = true;
        // rangeIndicatorTr.gameObject.SetActive(true);
        // rangeIndicatorTr.localScale = new Vector3(statMgr.AttackRange, statMgr.AttackRange, 1);
    }

    public void EndDrag(Tile tile)
    {
        IsBeingDragged = false;
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
    EliteBossDamage, // (2026-11-20) 엘리트·보스에게 주는 피해 +% (Add, 기본 0). 드워프 종족 버프 칸 등 → AllyUnitDamageData.eliteBossDamageMul
    // (2026-11-21) 방어 타입별 주는 피해 +% (Add, 기본 0, 버프로만). 종족 버프 → AllyUnitDamageData.GetDefenceDamageMul
    DamageToLight,    // 경갑 상대
    DamageToNormal,   // 중갑 상대
    DamageToArmored,  // 중장갑 상대
    DamageToEthereal, // 영체 상대
    SkillCooldownSpeed, // (2026-11-21) 미니언 스킬 쿨타임 충전 속도 +% (Add, 버프로만, 직업 마법사) → Minion.SkillDeltaTime
    DoubleShotChance, // (2026-11-23) 탄을 쏠 때 같은 탄을 한 발 더 쏠 확률 % (Add, 버프로만, 엘프 종족) → Minion.FireBullet
    
    Count
}

public class BulletFiredEvent
{
    public BulletSpec bullet;
    public Vector2 dir;
}


public class SlimeUpdateEvent
{
    public Slime slime;
    public SlimeUpdateEvent(Slime slime) { this.slime = slime; }
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

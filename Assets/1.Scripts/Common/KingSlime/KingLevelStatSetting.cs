using System;
using UnityEngine;

// (2026-11-24) 용사 강화 (코드 이름 KingLevelStat): 유저 레벨 1당 포인트 1 → 로비 킹 관리 화면에서 묶음(Tier)의 칸에 찍음. 언제든 무료로 초기화
// (2026-11-26) 묶음 = 레벨 구간: Lv.1~5에 얻은 포인트 → 1번 묶음 칸 3개, Lv.6~10 → 2번 … (구간 = unlockLevel부터 maxPoints 레벨, 포인트는 묶음끼리 안 섞임)
// 한 칸에 5포인트 전부 가능. 칸 = 킹 스탯(Stat) 또는 판 운영·보드 능력(Ability, HeroUpgradeAbility → 인게임 HeroUpgradeSystem). 1번 묶음 = 스탯, 2번부터 능력
// (이전 로비 재화 강화 KSUpgradeData·권능 Authority를 대체)
// Resources/KingLevelStatSetting 에셋 인스펙터에서 수정, 에셋이 없으면 코드 기본값(DEFAULT_TIERS, 임시 수치)
[CreateAssetMenu(fileName = "KingLevelStatSetting", menuName = "KingSlime/KingLevelStatSetting")]
public class KingLevelStatSetting : ScriptableObject
{
    public const string RESOURCE_PATH = "KingLevelStatSetting";
    public const int POINTS_PER_LEVEL = 1;

    public enum OptionKind { Stat, Ability }

    [Serializable]
    public class StatOption
    {
        public OptionKind kind;
        [Tooltip("kind = Stat일 때")]
        public StatType statType;
        [Tooltip("kind = Ability일 때 (번역 HeroUpgrade.{이름} / .Desc)")]
        public HeroUpgradeAbility ability;
        [Tooltip("ability = AttackVsDefence일 때: 이 공격 타입이")]
        public AttackType attackType;
        [Tooltip("ability = AttackVsDefence일 때: 이 방어 타입 적에게 주는 피해 +N%")]
        public DefenceType defenceType;
        [Tooltip("1포인트당 증가량 (실제 값). 예: 공격 범위 0.2. values가 있으면 values 사용")]
        public float valuePerPoint = 1f;
        [Tooltip("포인트별 값 (index = 포인트 - 1, 모자라면 마지막 값). 예: 5/6/7/8/9 (%). 비우면 valuePerPoint × 포인트")]
        public float[] values = new float[0];
        [Tooltip("켜면 % 곱연산: 기본값 × (1 + 포인트 × valuePerPoint / 100). 끄면 더하기")]
        public bool multiply;
        [Tooltip("화면 표시 배율. 예: 공격 범위 0.2 → ×100 = 20, 치명 피해 0.05 → ×100 = 5")]
        public float displayMul = 1f;
        [Tooltip("화면 표시 뒤에 붙는 글씨. 예: %")]
        public string suffix = "";

        public StatOption() { }
        public StatOption(StatType statType, float valuePerPoint, bool multiply = false, float displayMul = 1f, string suffix = "")
        {
            kind = OptionKind.Stat;
            this.statType = statType;
            this.valuePerPoint = valuePerPoint;
            this.multiply = multiply;
            this.displayMul = displayMul;
            this.suffix = suffix;
        }
        public static StatOption Ability(HeroUpgradeAbility ability, float valuePerPoint, string suffix = "", params float[] values) =>
            new StatOption { kind = OptionKind.Ability, ability = ability, valuePerPoint = valuePerPoint, suffix = suffix, values = values ?? new float[0] };
        // 공격 타입 → 방어 타입 상대 피해 +N%
        public static StatOption Versus(AttackType attackType, DefenceType defenceType, float valuePerPoint) =>
            new StatOption { kind = OptionKind.Ability, ability = HeroUpgradeAbility.AttackVsDefence, attackType = attackType, defenceType = defenceType, valuePerPoint = valuePerPoint, suffix = "%", values = new float[0] };

        // 찍은 포인트에서의 실제 값 (0포인트 = 0)
        public float ValueAt(int points)
        {
            if (points <= 0)
                return 0f;
            if (values != null && values.Length > 0)
                return values[Mathf.Min(points, values.Length) - 1];
            return valuePerPoint * points;
        }

        // 화면 표시: "+1", "+2%", "+20"
        public string GetValueText(int points)
        {
            float v = ValueAt(points) * displayMul;
            return $"+{v:0.##}{suffix}";
        }

        public string Title
        {
            get
            {
                if (kind != OptionKind.Ability)
                    return StatData.GetTitle(statType);
                if (ability == HeroUpgradeAbility.AttackVsDefence)
                    return string.Format(TranslateManager.GetText("HeroUpgrade.AttackVsDefence"), AttackInfo.Title(attackType), DefenceInfo.Title(defenceType));
                return TranslateManager.GetText($"HeroUpgrade.{ability}");
            }
        }

        // 능력 설명 (번역 HeroUpgrade.{이름}.Desc, {0} = 그 포인트의 값). 스탯은 "+1"
        public string GetDesc(int points)
        {
            if (kind != OptionKind.Ability)
                return GetValueText(Mathf.Max(1, points));
            float v = ValueAt(Mathf.Max(1, points)) * displayMul;
            if (ability == HeroUpgradeAbility.AttackVsDefence)
                return string.Format(TranslateManager.GetText("HeroUpgrade.AttackVsDefence.Desc"), v.ToString("0.##"), AttackInfo.Title(attackType), DefenceInfo.Title(defenceType));
            return string.Format(TranslateManager.GetText($"HeroUpgrade.{ability}.Desc"), v.ToString("0.##"));
        }
    }

    [Serializable]
    public class Tier
    {
        [Tooltip("이 묶음 구간의 첫 레벨 (이 레벨부터 maxPoints 레벨 동안 얻는 포인트가 이 묶음 몫)")]
        public int unlockLevel = 1;
        [Tooltip("이 묶음에 찍을 수 있는 포인트 합계 상한 = 구간 레벨 수")]
        public int maxPoints = 5;
        public StatOption[] options = new StatOption[0];

        public Tier() { }
        public Tier(int unlockLevel, int maxPoints, params StatOption[] options)
        {
            this.unlockLevel = unlockLevel;
            this.maxPoints = maxPoints;
            this.options = options;
        }
    }

    // DEFAULT_TIERS를 직접 넣지 않음: 같은 배열을 가리키면 에셋을 불러올 때 코드 기본값까지 에셋 값으로 덮어써짐 (새 에셋은 CreateAsset에서 복사)
    public Tier[] tiers = new Tier[0];

    // 임시 수치 (사용자가 에셋에서 조정). Lv.1~5 = 킹 스탯, Lv.6~부터 능력 (2026-11-24 사용자 확정 구성, 2026-11-26 구간 방식)
    // 빠진 능력(Lv.2 카드·경험치 보너스·막힌 칸·쌍둥이·시작 룬·모래시계·불굴·형태 다양·순혈·진화 공격력)은 코드만 남김 → 다시 쓰려면 칸에 넣기
    public static readonly Tier[] DEFAULT_TIERS =
    {
        // 킹
        new Tier(1, 5,
            new StatOption(StatType.AttackPower, 1f),
            new StatOption(StatType.AttackSpeed, 2f, multiply: true, suffix: "%"),
            new StatOption(StatType.AttackRange, 0.2f, displayMul: 100f)),
        // 정예병 (정예병 칸 미니언만)
        new Tier(6, 5,
            StatOption.Ability(HeroUpgradeAbility.EliteMinionAttack, 4f, "%"),
            StatOption.Ability(HeroUpgradeAbility.EliteMinionAttackSpeed, 3f, "%"),
            StatOption.Ability(HeroUpgradeAbility.EliteMinionSkillCooldown, 6f, "%")),
        // 전리품
        new Tier(11, 5,
            StatOption.Ability(HeroUpgradeAbility.EliteBossDiceChance, 1f, "%", 1, 2, 3, 4, 5),
            StatOption.Ability(HeroUpgradeAbility.BreakRerollChance, 1f, "%", 3, 4, 5, 6, 7),
            StatOption.Ability(HeroUpgradeAbility.RuneRewardLv2Chance, 1f, "%", 3, 4, 5, 6, 7)),
        // 병과
        new Tier(16, 5,
            StatOption.Ability(HeroUpgradeAbility.RangedAttackSpeed, 3f, "%"),
            StatOption.Ability(HeroUpgradeAbility.MeleeAttack, 5f, "%"),
            StatOption.Ability(HeroUpgradeAbility.MagicAttack, 4f, "%")),
        // 위기·상성
        new Tier(21, 5,
            StatOption.Ability(HeroUpgradeAbility.KingHitEnergyCharge, 5f),
            StatOption.Ability(HeroUpgradeAbility.KingHitEliteRage, 10f, "%"),
            StatOption.Ability(HeroUpgradeAbility.ResistEase, 5f, "%")),
        // 준비 (판 시작) (2026-11-25, 이전 Lv.30 상성(AttackVsDefence)은 코드만 남김)
        new Tier(26, 5,
            StatOption.Ability(HeroUpgradeAbility.StartDiceCoin, 1f),
            StatOption.Ability(HeroUpgradeAbility.MinionCostDownChance, 1f, "%"),
            StatOption.Ability(HeroUpgradeAbility.FirstOfferLv2Chance, 15f, "%")),
    };

    static KingLevelStatSetting instance;
    public static KingLevelStatSetting Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<KingLevelStatSetting>(RESOURCE_PATH);
            return instance;
        }
    }

    public static Tier[] Tiers
    {
        get
        {
            var s = Instance;
            return s != null && s.tiers != null && s.tiers.Length > 0 ? s.tiers : DEFAULT_TIERS;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Data/Create KingLevelStatSetting")]
    static void CreateAsset()
    {
        const string path = "Assets/Resources/" + RESOURCE_PATH + ".asset";
        var existing = UnityEditor.AssetDatabase.LoadAssetAtPath<KingLevelStatSetting>(path);
        if (existing != null)
        {
            UnityEditor.Selection.activeObject = existing;
            return;
        }
        var asset = CreateInstance<KingLevelStatSetting>();
        var defaults = CreateInstance<KingLevelStatSetting>();
        defaults.tiers = DEFAULT_TIERS;
        UnityEditor.EditorUtility.CopySerialized(defaults, asset); // 기본값을 복사 (같은 배열 공유 안 함)
        DestroyImmediate(defaults);
        UnityEditor.AssetDatabase.CreateAsset(asset, path);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.Selection.activeObject = asset;
    }
#endif
}

// (2026-11-24) 용사 강화 능력 (Lv.10부터). 저장·에셋에 번호로 들어가므로 뒤에만 추가. 값의 뜻은 HeroUpgradeSystem 참고
public enum HeroUpgradeAbility
{
    StartDiceCoin,          // 첫 브레이크타임 주사위 코인 +N
    OfferLv2Chance,         // Lv.2 카드 확률 +N% (판 내내)
    FirstOfferLv2Chance,    // 첫 브레이크타임 카드 Lv.2 확률 +N%
    EliteBossDiceChance,    // 엘리트·보스 처치 시 N% 확률로 다음 브레이크타임 주사위 코인 +1
    ExpBuyBonusChance,      // 킹 경험치 구매 시 N% 확률로 경험치 +1 더
    BreakRerollChance,      // 브레이크타임마다 N% 확률로 카드 다시 뽑기 무료 1회 (쌓이지 않음)
    StartUnblockTile,       // 시작 시 막힌 칸 N칸 해제
    TwinAttack,             // 같은 그룹 상하좌우에 같은 종이 있으면 미니언 공격력 +N%
    StartRuneChance,        // 시작 시 N% 확률로 첫 브레이크타임 룬 선택 1회
    LowHpFreeze,            // 킹 체력이 처음 30% 아래가 되면 모든 적 N초 정지
    LastStand,              // 판당 1번, 체력이 0이 되면 최대 체력 N%로 버팀
    FormVarietyAttackSpeed, // 장착 5종 형태가 모두 다르면 미니언 공속 +N%
    PureTypeWeakness,       // 장착 5종 공격 타입이 모두 같으면 약점 피해 +N%
    EvolvedAttack,          // 5성 진화한 미니언 공격력 +N%
    // (2026-11-24 2차 구성)
    EliteMinionAttack,        // 정예병 칸 미니언 공격력 +N%
    EliteMinionAttackSpeed,   // 정예병 칸 미니언 공속 +N%
    EliteMinionSkillCooldown, // 정예병 칸 미니언 스킬 쿨타임 충전 +N%
    RuneRewardLv2Chance,      // 룬 보상(엘리트·보스)으로 새기거나 올릴 때 N% 확률로 1레벨 더
    RangedAttackSpeed,        // 원거리(발사형) 미니언 공속 +N%
    MeleeAttack,              // 근거리(근접·회전형) 미니언 공격력 +N%
    MagicAttack,              // 마법 미니언 공격력 +N%
    KingHitEnergyCharge,      // 킹이 피해를 받으면 피해 1당 용사 에너지 충전 N초
    KingHitEliteRage,         // 킹이 피해를 받으면 정예병 칸 미니언 3초간 공격력 +N%
    ResistEase,               // 아군 공격의 내성 배율(1 미만) +N%p (최대 100%)
    AttackVsDefence,          // 공격 타입 → 방어 타입 상대 피해 +N% (칸의 attackType·defenceType)
    MinionCostDownChance,     // (2026-11-25) 카드를 줄 때 장마다 N% 확률로 가격(주사위 코인) -1 (가격 1 이하 카드는 그대로)
}

using System.Collections.Generic;
using UnityEngine;

// (2026-11-23) 용사 = 종족 + 직업을 하나로 합친 킹 성장 (Lv.1에 고름, 한 판에 하나). "용사를 동경하는 슬라임"
// (2026-11-24) 개편:
// - 정예병 칸(버프 칸) = 카드를 보여줄 때 킹 주변 8칸 중 랜덤 2칸 (카드마다 다름, LevelUpChoiceRule.PickHeroTiles → 고르면 eliteOffsets에 넣음)
//   칸 버프(eliteBuffs)는 정예병 칸 미니언만 받음 (킹 X)
// - (2026-11-25) 용사 레벨 = 킹 레벨(레벨업 창을 다 처리한 레벨, HeroLevel). 레벨업마다 킹 스탯(HeroData Add*)·정예병 칸 버프(InitBuffValue + AddBuffValue × (Lv-1)) 자동 상승
//   용사 카드는 다시 고르지 않음(maxLevel 1). 용사 강화 단계 카드 = 정예 칸 추가 · 충전 가속 (KingGrowthSetting.heroBoostAbilities)
// - 스킬 3단계: Lv.2 = skills[0], Lv.5 = skills[1], Lv.8 = 공용 "용사가 될거야!" (모든 필드 미니언 버프). 해방 = UnlockNextSkill
// - 에너지: wave 중 충전 시간(30초)마다 1개, 최대 = 1단계 해방 때 2개 + 에너지 보유 증가 카드 (자동 증가 없음)
//   (2026-11-25 삭제: 정예병 칸 미니언·킹 처치로 충전 시간 감소) 충전 가속 카드(HeroEnergyKingAbility) = 충전 시간 -10%/Lv
//   하단 스킬 패널(HeroSkillHud)을 누르면 에너지 1개를 쓰고 발동. 대상이 없으면 발동 안 함(에너지 그대로)
// - 에너지 컨셉(이름·아이콘·색)은 용사마다 (기사 = 투지, 궁수 = 집중, 사령술사 = 어둠)
// - (2026-11-25) 기본 공격을 탄 대신 바꿀 수 있음 (TryKingAttack: 기사 = 근거리 휘두르기, 사령술사 = 원거리 슬래쉬)
//   위치 지정 스킬 (NeedsTargetPosition): 버튼 → "원하는 위치를 누르세요" → 필드를 누른 곳에 발동 (TryUseSkillAt)
//   용사가 될거야! = 10초 모든 미니언·킹 공격력·공속 ×1.5
// 새 용사 = 하위 클래스 하나(CastSkill·GetSkillText) + 프리팹 + AbilityData(category Hero) → KingGrowthSetting.heroAbilities에 추가
public abstract class HeroKingAbility : KingAbility
{
    public const int SKILL_COUNT = 3;
    public const int COMMON_SKILL_INDEX = 2;
    public const string COMMON_SKILL_NAME = "용사가 될거야!";

    public KingHeroType heroType;
    [Tooltip("HeroData.csv key. 비우면 클래스 이름에서 Hero를 뺀 것 (KnightHero → Knight)")]
    [SerializeField] string heroDataKey;
    public string HeroDataKey => !string.IsNullOrEmpty(heroDataKey) ? heroDataKey : GetType().Name.Replace("Hero", "");
    [Tooltip("정예병 칸 (Tile.index 기준: x 오른쪽 +, y 아래쪽 +). 레벨업 카드에서 랜덤 2칸으로 덮어씀, 여기 값은 테스트·기본값")]
    [UnityEngine.Serialization.FormerlySerializedAs("tileOffsets")]
    public Vector2Int[] eliteOffsets = { new Vector2Int(-1, 0), new Vector2Int(1, 0) };
    [Tooltip("정예병 칸 미니언만 받음 (킹 X). 스탯·곱/더하기는 첫 칸 기준, 값은 HeroData.csv InitBuffValue·AddBuffValue (표에 없으면 여기 값 그대로)")]
    [UnityEngine.Serialization.FormerlySerializedAs("tileBuffs")]
    public Buff[] eliteBuffs = { new Buff(StatType.AttackPower, 1.1f, StatOpType.Multiply) };
    public Color tileColor = new Color(1f, 0.85f, 0.3f, 0.35f);
    public AttackType kingAttackType = AttackType.Physic;

    [Header("에너지 (용사마다 컨셉)")]
    public string energyName = "에너지";
    public Sprite energyIcon;
    public Color energyColor = new Color(0.55f, 0.85f, 1f, 1f);

    [Header("스킬 (1단계 = Lv.2, 2단계 = Lv.5)")]
    public HeroSkillInfo[] skills = { new HeroSkillInfo { name = "스킬 1" }, new HeroSkillInfo { name = "스킬 2" } };

    [Header("3단계 공용 스킬: 용사가 될거야!")]
    public Sprite commonSkillIcon;
    [Tooltip("용사가 될거야! 소모 에너지")]
    [Min(1)] public int commonSkillCost = 1;
    [Tooltip("HeroData.csv Skill3Value가 있으면 그 값 (0 = 지속 시간, 1 = 공격력·공속 증가 비율: 2.5 = +250% = ×3.5)")]
    public float commonDuration = 10f;
    public float commonAttackPowerMul = 1.5f;
    public float commonAttackSpeedMul = 1.5f;
    public float CommonDuration => SkillValue(COMMON_SKILL_INDEX, 0, commonDuration);
    public float CommonAttackPowerMul => 1f + SkillValue(COMMON_SKILL_INDEX, 1, commonAttackPowerMul - 1f);
    public float CommonAttackSpeedMul => 1f + SkillValue(COMMON_SKILL_INDEX, 1, commonAttackSpeedMul - 1f);

    // (2026-11-25) 용사 강화 카드 "1·2단계 스킬 강화"를 골랐는지 (HeroSkillEnhanceKingAbility)
    public bool IsSkillEnhanced(int idx)
    {
        if (king == null || king.abilityInventory == null)
            return false;
        foreach (var a in king.abilityInventory.Abilities)
            if (a is HeroSkillEnhanceKingAbility s && s.skillIndex == idx && s.level > 0)
                return true;
        return false;
    }

    // (2026-11-25) 스킬 수치 = HeroData.csv SkillNValue의 param번째 (스킬 강화 카드를 골랐으면 강화 값). 표에 없으면 fallback(프리팹 값)
    protected float SkillValue(int skillIdx, int param, float fallback) => SkillValue(skillIdx, param, fallback, IsSkillEnhanced(skillIdx));

    float SkillValue(int skillIdx, int param, float fallback, bool enhanced)
    {
        HeroDataTable.HeroSkill s = GetSkillData(skillIdx);
        if (s == null || param < 0 || param >= s.values.Count)
            return fallback;
        return enhanced && param < s.enhancedValues.Count ? s.enhancedValues[param] : s.values[param];
    }

    HeroDataTable.HeroSkill GetSkillData(int skillIdx)
    {
        HeroDataTable.HeroData d = HeroData;
        return d != null && skillIdx >= 0 && skillIdx < d.skills.Count ? d.skills[skillIdx] : null;
    }

    // HeroData 스킬 설명 ({n} = 값, 뒤에 %가 붙으면 ×100). 없으면 null
    string FormatSkillDesc(int idx, System.Func<int, string> valueText)
    {
        HeroDataTable.HeroSkill s = GetSkillData(idx);
        if (s == null || string.IsNullOrEmpty(s.desc))
            return null;
        string desc = s.desc;
        if (idx == COMMON_SKILL_INDEX && desc.StartsWith(COMMON_SKILL_NAME))
            desc = desc.Substring(COMMON_SKILL_NAME.Length).TrimStart();
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < desc.Length; i++)
        {
            int close = desc[i] == '{' ? desc.IndexOf('}', i) : -1;
            if (close > i && int.TryParse(desc.Substring(i + 1, close - i - 1), out int n))
            {
                sb.Append(valueText(n));
                i = close;
                continue;
            }
            sb.Append(desc[i]);
        }
        return sb.ToString();
    }

    bool IsPercentParam(int idx, int n)
    {
        HeroDataTable.HeroSkill s = GetSkillData(idx);
        string key = "{" + n + "}";
        int at = s != null ? s.desc.IndexOf(key, System.StringComparison.Ordinal) : -1;
        return at >= 0 && at + key.Length < s.desc.Length && s.desc[at + key.Length] == '%';
    }

    string ParamText(int idx, int n, bool enhanced)
    {
        float v = SkillValue(idx, n, 0f, enhanced);
        return Num(IsPercentParam(idx, n) ? v * 100f : v);
    }

    // 카드 설명: 바뀌는 값만 "5 → 7" (강화 값 초록)
    public string GetSkillEnhanceText(int idx)
    {
        string text = FormatSkillDesc(idx, n =>
        {
            string from = ParamText(idx, n, false), to = ParamText(idx, n, true);
            return from == to ? from : $"{from} → <color=#7CFF6B>{to}</color>";
        });
        return text ?? "스킬 강화";
    }

    // ---- 에너지
    public int UnlockedSkillCount { get; private set; }
    // (2026-11-25) 최대 개수 = 2 + 에너지 보유 증가 카드, 스킬을 하나도 해방하지 않았으면 0 (킹 레벨로 자동 증가 없음)
    public int MaxCharges => UnlockedSkillCount > 0 && king != null
        ? GameSetting.HERO_ENERGY_BASE_MAX + Mathf.RoundToInt(SumEnergyBonus(HeroEnergyBoostType.MaxCharges)) : 0;
    public int Charges { get; private set; }
    public float ChargeProgress { get; private set; } // 지금 충전 중인 1개에 쌓인 시간(초)
    public float ChargeRatio => ChargeTime > 0 ? Mathf.Clamp01(ChargeProgress / ChargeTime) : 0f;
    public bool IsFull => Charges >= MaxCharges;
    public int UseCount { get; private set; } // 스킬 사용 횟수 (테스트·표시용)

    EliteArea area;
    protected EliteArea Area => area != null ? area : (area = EliteArea.GetOrCreate(king));
    AttackType prevAttackType;

    readonly UnitBuffGroup commonBuff = new UnitBuffGroup();
    public bool IsCommonSkillActive => commonBuff.IsActive;

    // (2026-11-25) 아군 전체(미니언·킹)의 타겟 방식을 잠깐 바꿈 (궁수 매의 눈 = 높은 체력). null = 각자 설정
    public static FindTargetType? ForcedAllyFindTarget { get; protected set; }

    // ---- (2026-11-25) 용사 레벨 = 킹 레벨 (레벨업 창까지 다 처리한 레벨, 최소 1)
    public static int GetHeroLevel(KingSlime king) => king != null ? Mathf.Max(1, king.lv - king.pendingLevelUps) : 1;
    public int HeroLevel => GetHeroLevel(king);
    int appliedHeroLevel;
    public HeroDataTable.HeroData HeroData => HeroDataTable.Get(HeroDataKey);

    public override void OnEquip()
    {
        // (2026-11-24) 킹 기본 스탯 = HeroData.csv의 이 용사 행 (없으면 KingSlimeData 그대로)
        if (HeroData == null)
            Debug.LogWarning($"[HeroKingAbility] HeroData.csv에 '{HeroDataKey}' 없음 → 킹 기본 스탯(KingSlimeData) 유지");
        Area.SetShape(this, eliteOffsets, tileColor);
        appliedHeroLevel = 0;
        RefreshHeroLevel();
        if (king.weapon != null)
        {
            prevAttackType = king.weapon.attackType;
            king.weapon.attackType = kingAttackType;
        }
        OnHeroEquip();
    }

    // 레벨업 처리마다 (KingSlime.ConsumeLevelUp): 킹 스탯·정예병 칸 버프를 그 레벨 값으로
    public void RefreshHeroLevel()
    {
        int lv = HeroLevel;
        if (lv == appliedHeroLevel)
            return;
        appliedHeroLevel = lv;
        HeroDataTable.HeroData heroData = HeroData;
        if (heroData != null)
            king.ApplyHeroBaseStats(heroData, lv);
        ApplyTileBuffs();
        OnHeroLevelChanged();
    }

    public override void OnUnequip()
    {
        if (king.statMgr.heroData != null)
            king.ApplyHeroBaseStats(null);
        EndCommonSkill();
        if (area != null)
        {
            area.RemoveEliteBuffs(this);
            area.RemoveShape(this);
        }
        if (king.weapon != null)
            king.weapon.attackType = prevAttackType;
        if (addedMinionCount != 0 && MinionFieldController.Instance != null)
            MinionFieldController.Instance.minionMaxCount -= addedMinionCount;
        addedMinionCount = 0;
        OnHeroUnequip();
    }

    protected virtual void OnDestroy()
    {
        EndCommonSkill();
        ForcedAllyFindTarget = null;
    }

    void ApplyTileBuffs() => Area.SetEliteBuffs(this, GetEliteBuffs(HeroLevel));

    // 용사 레벨 lv의 정예병 칸 버프 (HeroData.csv 값, 없으면 프리팹 값)
    public List<Buff> GetEliteBuffs(int lv)
    {
        HeroDataTable.HeroData heroData = HeroData;
        if (heroData == null || !heroData.HasEliteBuff || eliteBuffs == null || eliteBuffs.Length == 0)
        {
            var list = new List<Buff>();
            if (eliteBuffs != null)
                foreach (Buff t in eliteBuffs)
                    list.Add(new Buff(t.statType, t.value, t.opType) { effectType = t.effectType });
            return list;
        }
        Buff b = eliteBuffs[0];
        float v = heroData.GetEliteBuffValue(lv);
        return new List<Buff> { new Buff(b.statType, b.opType == StatOpType.Multiply ? 1f + v / 100f : v, b.opType) { effectType = b.effectType } };
    }

    // "공격력 +20%" (HeroData EliteBuffDesc의 {0} = 값, 없으면 버프 글씨)
    public string GetEliteBuffText(int lv)
    {
        HeroDataTable.HeroData heroData = HeroData;
        if (heroData != null && heroData.HasEliteBuff && !string.IsNullOrEmpty(heroData.eliteBuffDesc))
        {
            try { return string.Format(heroData.eliteBuffDesc, Num(heroData.GetEliteBuffValue(lv))); }
            catch (System.FormatException) { }
        }
        return EliteArea.GetBuffText(GetEliteBuffs(lv));
    }

    protected virtual void OnHeroEquip() { }
    protected virtual void OnHeroLevelChanged() { }
    protected virtual void OnHeroUnequip() { }

    #region 에너지
    // (2026-11-25) 충전 시간 = 30초 × (1 - 충전 가속 카드 %) (최소 HERO_MIN_CHARGE_TIME). 이전: -N초
    public float ChargeTime => Mathf.Max(GameSetting.HERO_MIN_CHARGE_TIME,
        GameSetting.HERO_ENERGY_CHARGE_TIME * (1f - SumEnergyBonus(HeroEnergyBoostType.ChargeTime) / 100f));

    float SumEnergyBonus(HeroEnergyBoostType type)
    {
        float sum = 0f;
        if (king == null || king.abilityInventory == null)
            return sum;
        foreach (var a in king.abilityInventory.Abilities)
            if (a is HeroEnergyKingAbility e && e.boostType == type)
                sum += e.GetValue(e.level);
        return sum;
    }

    // 충전은 wave 중에만 (브레이크타임엔 멈춤)
    public static bool IsCharging => GameManager.Instance != null && GameManager.Instance.isPlaying && !GameManager.Instance.isBreakTime;

    protected virtual void Update()
    {
        if (IsCharging)
            AddChargeTime(Time.deltaTime);
        if (commonBuff.Tick(Time.deltaTime))
            commonBuff.Remove();
    }

    // 충전 시간 추가 (넘치면 다음 칸으로 이월, 가득 차면 멈춤)
    public void AddChargeTime(float seconds)
    {
        if (seconds <= 0f || MaxCharges <= 0 || IsFull)
            return;
        ChargeProgress += seconds;
        while (!IsFull && ChargeProgress >= ChargeTime)
        {
            ChargeProgress -= ChargeTime;
            Charges++;
        }
        if (IsFull)
            ChargeProgress = 0f;
    }

    // 테스트·도구용: 에너지 바로 채움
    public void FillChargesForTest()
    {
        Charges = MaxCharges;
        ChargeProgress = 0f;
    }
    #endregion

    #region 스킬
    // 2단계 스킬(Lv.5)을 해방하면 필드 최대 미니언 수 +1
    public const int SKILL2_MINION_COUNT_ADD = 1;
    int addedMinionCount;

    // 다음 단계 스킬 해방 (Lv.2·5·8). 에너지 최대 개수 +1 (지금 충전 중인 시간은 이어서)
    public void UnlockNextSkill()
    {
        if (UnlockedSkillCount >= SKILL_COUNT)
            return;
        UnlockedSkillCount++;
        if (UnlockedSkillCount == 2 && MinionFieldController.Instance != null)
        {
            MinionFieldController.Instance.minionMaxCount += SKILL2_MINION_COUNT_ADD;
            addedMinionCount += SKILL2_MINION_COUNT_ADD;
        }
    }

    // 다음에 해방할 스킬 (없으면 -1)
    public int NextSkillIndex => UnlockedSkillCount < SKILL_COUNT ? UnlockedSkillCount : -1;

    public string GetSkillName(int idx)
    {
        if (idx == COMMON_SKILL_INDEX)
            return COMMON_SKILL_NAME;
        return skills != null && idx >= 0 && idx < skills.Length ? skills[idx].name : "";
    }

    public Sprite GetSkillIcon(int idx)
    {
        if (idx == COMMON_SKILL_INDEX)
            return commonSkillIcon != null ? commonSkillIcon : data != null ? data.thum : null;
        Sprite s = skills != null && idx >= 0 && idx < skills.Length ? skills[idx].icon : null;
        return s != null ? s : data != null ? data.thum : null;
    }

    // (2026-11-25) 설명 = HeroData.csv SkillN (지금 값, 강화했으면 강화 값). 없으면 코드 글씨
    public string GetSkillDesc(int idx) => FormatSkillDesc(idx, n => ParamText(idx, n, IsSkillEnhanced(idx)))
        ?? (idx == COMMON_SKILL_INDEX
            ? $"{Num(CommonDuration)}초간 모든 미니언·킹 공격력 ×{Num(CommonAttackPowerMul)}, 공속 ×{Num(CommonAttackSpeedMul)}"
            : GetSkillText(idx));

    public bool IsSkillUnlocked(int idx) => idx >= 0 && idx < UnlockedSkillCount;

    // (2026-11-24) 스킬마다 소모 에너지 (하단 스킬 버튼에 아이콘 개수로 표시). 최대 개수보다 크면 못 씀
    public int GetSkillCost(int idx)
    {
        if (idx == COMMON_SKILL_INDEX)
            return Mathf.Max(1, commonSkillCost);
        return skills != null && idx >= 0 && idx < skills.Length ? Mathf.Max(1, skills[idx].cost) : 1;
    }

    public enum UseResult { Used, Locked, NoEnergy, NotNow, NoTarget, NeedPosition }

    // (2026-11-25) 위치 지정 스킬 (버튼을 누른 뒤 필드를 눌러 발동)
    public virtual bool NeedsTargetPosition(int idx) => false;
    // 위치 지정 스킬의 범위 (조준 표시용, 0 = 표시 없음)
    public virtual float GetTargetRadius(int idx) => 0f;

    // 하단 스킬 패널을 누르면. 에너지 GetSkillCost개 소비. 위치 지정 스킬이면 NeedPosition (에너지 그대로) → TryUseSkillAt
    public UseResult TryUseSkill(int idx)
    {
        UseResult check = CheckUsable(idx);
        if (check != UseResult.Used)
            return check;
        if (idx != COMMON_SKILL_INDEX && NeedsTargetPosition(idx))
            return UseResult.NeedPosition;
        bool cast = idx == COMMON_SKILL_INDEX ? CastCommonSkill() : CastSkill(idx);
        return FinishUse(idx, cast);
    }

    // 위치 지정 스킬 발동 (pos = 월드 위치)
    public UseResult TryUseSkillAt(int idx, Vector2 pos)
    {
        UseResult check = CheckUsable(idx);
        if (check != UseResult.Used)
            return check;
        bool cast = NeedsTargetPosition(idx) ? CastSkillAt(idx, pos) : (idx == COMMON_SKILL_INDEX ? CastCommonSkill() : CastSkill(idx));
        return FinishUse(idx, cast);
    }

    UseResult CheckUsable(int idx)
    {
        if (!IsSkillUnlocked(idx))
            return UseResult.Locked;
        if (!IsCharging)
            return UseResult.NotNow;
        if (Charges < GetSkillCost(idx))
            return UseResult.NoEnergy;
        return UseResult.Used;
    }

    UseResult FinishUse(int idx, bool cast)
    {
        if (!cast)
            return UseResult.NoTarget;
        int cost = GetSkillCost(idx);
        Charges -= cost;
        UseCount++;
        Color c = energyColor;
        c.a = 1f;
        EffectManager.PlayPopText((Vector2)king.transform.position + Vector2.up * 1.2f, $"{GetSkillName(idx)}!", c);
        return UseResult.Used;
    }

    // idx 0 = 1단계, 1 = 2단계. 대상이 없어 못 썼으면 false (에너지 안 씀)
    protected abstract bool CastSkill(int idx);
    protected virtual bool CastSkillAt(int idx, Vector2 pos) => CastSkill(idx);

    // (2026-11-25) 킹 기본 공격을 탄 대신 직접 처리하면 true (Weapon.Attack). dir = 타겟 방향
    public virtual bool TryKingAttack(Vector2 dir) => false;
    protected abstract string GetSkillText(int idx);
    protected abstract string GetAttackFormText();

    // 용사가 될거야!: 지금 필드의 모든 미니언 + 킹 버프 (끝나면 붙인 버프만 뗌. 다시 쓰면 시간만 처음부터)
    bool CastCommonSkill()
    {
        Fx(EffectType.Flame, king.transform.position); // 임시 이펙트
        Fx(EffectType.Explosion, king.transform.position);
        commonBuff.Apply(king, CommonDuration, () => new[]
        {
            new Buff(StatType.AttackPower, CommonAttackPowerMul, StatOpType.Multiply) { effectType = EffectType.AttackPowerUp },
            new Buff(StatType.AttackSpeed, CommonAttackSpeedMul, StatOpType.Multiply) { effectType = EffectType.AttackSpeedUp },
        });
        return true;
    }

    void EndCommonSkill() => commonBuff.Remove();
    #endregion

    // ---- 카드 설명 ({0} = 전체 설명)
    public override string GetValueText(int level) =>
        $"정예병 칸 미니언: {GetEliteBuffText(king != null ? HeroLevel : 1)} (레벨업마다 상승)\n"
        + $"킹 공격: {GetAttackTypeName(kingAttackType)} · {GetAttackFormText()}\n"
        + $"에너지: <color=#{ColorUtility.ToHtmlStringRGB(energyColor)}>{energyName}</color>\n"
        + $"Lv.2 <b>{GetSkillName(0)}</b> · Lv.5 <b>{GetSkillName(1)}</b>";

    public static string GetAttackTypeName(AttackType type)
    {
        switch (type)
        {
            case AttackType.Physic: return "물리";
            case AttackType.Magic: return "마법";
            case AttackType.Vibration: return "진동";
            default: return "카오스";
        }
    }

    #region 스킬 공용 도우미
    protected float KingAttackPower => king != null && king.statMgr != null ? king.statMgr.AttackPower : 0f;

    protected static List<Enemy> AliveEnemies()
    {
        // (2026-11-25) 보스 포함 (보스는 activeEnemies에 없음)
        if (EnemyFieldController.Instance == null)
            return new List<Enemy>();
        return EnemyFieldController.Instance.GetTargetEnemies();
    }

    // 체력이 높은 순서로 count명
    protected static List<Enemy> HighestHpEnemies(int count)
    {
        var list = AliveEnemies();
        list.Sort((a, b) => b.CurHp.CompareTo(a.CurHp));
        if (list.Count > count)
            list.RemoveRange(count, list.Count - count);
        return list;
    }

    // 주변(radius) 적이 가장 많은 적 위치
    public static bool FindDensest(float radius, out Vector2 pos)
    {
        pos = default;
        var enemies = AliveEnemies();
        int best = 0;
        float sqr = radius * radius;
        foreach (Enemy a in enemies)
        {
            int n = 0;
            foreach (Enemy b in enemies)
                if (((Vector2)(a.transform.position - b.transform.position)).sqrMagnitude <= sqr)
                    n++;
            if (n > best)
            {
                best = n;
                pos = a.transform.position;
            }
        }
        return best > 0;
    }

    // 범위 피해 (킹 공격, 킹 공격 타입). 맞아 죽으면 activeEnemies가 바뀌므로 복사본으로
    protected List<Enemy> DamageArea(Vector2 pos, float radius, float damage, string tag = null)
    {
        var targets = new List<Enemy>();
        targets.AddRange(EnemiesInRange(pos, radius));
        foreach (Enemy e in targets)
            DamageEnemy(e, damage, false, false, default, tag);
        return targets;
    }

    // (2026-11-25) 피해 집계 구분 (DamageMeterSystem 킹 피해: 기본 공격 / 1·2단계 스킬)
    public const string TAG_BASIC = "Basic", TAG_SKILL1 = "Skill1", TAG_SKILL2 = "Skill2";

    // canCrit = 킹 치명타 확률·배율 적용 (기본 공격을 대신하는 휘두르기·슬래쉬)
    // applyRune = (2026-11-25) 킹 칸 룬의 적중 효과 (치명타·만능·거인 사냥 등 피해 수정 + 슬로우·스턴·처치 코인 등 적중 후 효과). 탄 없는 기본 공격용
    public void DamageEnemy(Enemy e, float damage, bool canCrit = false, bool applyRune = false, Vector2 dir = default, string tag = null)
    {
        if (e == null || e.CurHp <= 0 || king == null)
            return;
        var data = new AllyUnitDamageData();
        data.Init(king);
        data.attackType = king.AttackType;
        data.damage = damage;
        data.meterTag = tag;
        if (canCrit && king.statMgr != null)
        {
            data.critChance = king.statMgr.CritChance;
            data.critPower = king.statMgr.CritPower;
        }
        TileItem rune = applyRune ? KingRune : null;
        if (rune != null)
            rune.ModifyDamageData(data);
        e.TakeDamage(data);
        if (rune != null)
        {
            IBulletBehavior hit = rune.HitBehavior; // 확률형은 읽을 때 판정하므로 한 번만
            hit?.OnHit(null, e, new RaycastHit2D(), dir);
        }
    }

    // ---- (2026-11-25) 킹 칸 룬을 탄 없는 기본 공격에 (기사 휘두르기·사령술사 슬래쉬)
    protected TileItem KingRune => king != null && king.shotModifier != null ? king.shotModifier.tileItem : null;
    // 연발 룬: 같은 공격을 burstCount번 더 (간격 COMBO_ATTACK_INTERVAL_MS)
    protected void RunRuneBurst(System.Action attack)
    {
        if (KingRune is BurstShotTileItem burst && burst.burstCount > 0 && attack != null)
            StartCoroutine(BurstRoutine(attack, burst.burstCount));
    }

    System.Collections.IEnumerator BurstRoutine(System.Action attack, int count)
    {
        var wait = new WaitForSeconds(KingSlime.COMBO_ATTACK_INTERVAL_MS / 1000f);
        for (int i = 0; i < count; i++)
        {
            yield return wait;
            if (king == null || GameManager.Instance == null || !GameManager.Instance.isPlaying)
                yield break;
            attack();
        }
    }

    // 다중 타겟: 첫 타겟(무기가 고른 적) + 서로 다른 추가 타겟 (count개까지)
    protected List<Transform> GetMultiTargets(int count)
    {
        var list = new List<Transform>();
        if (king.weapon == null)
            return list;
        Transform first = king.weapon.FindTarget();
        if (first != null)
            list.Add(first);
        if (count > 1)
            foreach (Transform t in InGameUtil.FindTargets(king.transform.position, king.statMgr.AttackRange + 0.3f, king.weapon.targetLayerMask, king.EffectiveFindTargetType, count))
                if (t != null && !list.Contains(t) && list.Count < count)
                    list.Add(t);
        return list;
    }

    // (2026-11-25) 임시 이펙트 (공용 EffectManager 이펙트 재사용)
    protected static void Fx(EffectType type, Vector2 pos)
    {
        if (EffectManager.Instance != null)
            EffectManager.Instance.Play(type, pos);
    }

    // 다중 타겟 룬·궁수 등 bonusShootCount: 첫 방향 + 서로 다른 추가 타겟 방향
    protected List<Vector2> GetMultiTargetDirs(Vector2 firstDir)
    {
        var dirs = new List<Vector2> { firstDir };
        int count = 1 + (king.shotModifier != null ? king.shotModifier.bonusShootCount : 0);
        if (count <= 1 || king.weapon == null)
            return dirs;
        List<Transform> targets = InGameUtil.FindTargets(king.transform.position, king.statMgr.AttackRange + 0.3f, king.weapon.targetLayerMask, king.EffectiveFindTargetType, count);
        for (int i = 1; i < Mathf.Min(count, targets.Count); i++)
            dirs.Add(((Vector2)(targets[i].position - king.transform.position)).normalized);
        return dirs;
    }

    // 범위 안 적 (보스 포함)
    protected static List<Enemy> EnemiesInRange(Vector2 pos, float radius)
    {
        var list = new List<Enemy>();
        float sqr = radius * radius;
        foreach (Enemy e in AliveEnemies())
            if (((Vector2)e.transform.position - pos).sqrMagnitude <= sqr)
                list.Add(e);
        return list;
    }

    public KingSlime King => king;
    public float CurrentKingAttackPower => KingAttackPower;

    public static void ApplyStatus(Enemy e, StatusEffect effect)
    {
        if (e != null && e.CurHp > 0)
            e.ApplyStatusEffect(effect); // 상태이상 처리 컴포넌트가 없는 적이면 붙여서 적용
    }
    #endregion

    protected static string Num(float v) => FormatNumber(v);
}

[System.Serializable]
public class HeroSkillInfo
{
    public string name;
    public Sprite icon;
    [Tooltip("소모 에너지")]
    [Min(1)] public int cost = 1;
}

// 용사 (에셋·저장에 번호로 → 뒤에만 추가)
public enum KingHeroType
{
    HumanKnight,
    ElfArcher,
    UndeadNecromancer,
}

// (2026-11-25) 필드의 모든 미니언 + 킹에게 일정 시간 버프 (용사가 될거야!·매의 눈). 끝나면 붙인 버프만 뗌, 다시 쓰면 시간만 처음부터
public class UnitBuffGroup
{
    readonly List<(Minion minion, Buff buff)> minionBuffs = new List<(Minion, Buff)>();
    readonly List<Buff> kingBuffs = new List<Buff>();
    KingSlime king;
    float timer;
    public bool IsActive => timer > 0f;

    public void Apply(KingSlime king, float duration, System.Func<Buff[]> makeBuffs)
    {
        Remove();
        this.king = king;
        if (MinionFieldController.Instance != null)
            foreach (Minion m in MinionFieldController.Instance.activeminions)
            {
                if (m == null || !m.isActiveAndEnabled)
                    continue;
                foreach (Buff b in makeBuffs())
                {
                    m.AddBuff(b);
                    minionBuffs.Add((m, b));
                }
            }
        if (king != null)
            foreach (Buff b in makeBuffs())
            {
                king.AddBuff(b);
                kingBuffs.Add(b);
            }
        timer = duration;
    }

    // 시간이 다 되는 순간 true
    public bool Tick(float dt)
    {
        if (timer <= 0f)
            return false;
        timer -= dt;
        return timer <= 0f;
    }

    public void Remove()
    {
        timer = 0f;
        foreach (var (m, b) in minionBuffs)
            if (m != null && m.activeBuffs.Contains(b))
                m.RemoveBuff(b);
        minionBuffs.Clear();
        if (king != null)
            foreach (Buff b in kingBuffs)
                king.RemoveBuff(b);
        kingBuffs.Clear();
    }
}

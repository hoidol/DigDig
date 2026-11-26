using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

// (2026-11-24) 용사 강화 능력(로비에서 찍은 포인트, Lv.10 묶음부터)을 인게임에 적용. 값 = UserKingSlimeManager.GetAbilityValue
// 게임 시작(GameManager.Start, 타일·킹 배치 후)에 Begin → 값을 읽어 두고 시작 효과(주사위 코인·막힌 칸·룬) 적용
// 나머지는 각 기능이 여기 함수를 부름: 카드 확률(MinionOfferSystem) · 처치 주사위(RuneRewardSystem) · 경험치 구매(KingSlime)
// · 체력(KingSlimeHealth) · 미니언 공격력·공속(Minion.UpdateMinion) · 약점 배율(EnemyDefence)
public static class HeroUpgradeSystem
{
    public const float LOW_HP_RATIO = 0.3f; // LowHpFreeze 발동 체력 비율

    public const float ELITE_RAGE_DURATION = 3f; // KingHitEliteRage 지속 (초)

    static readonly Dictionary<HeroUpgradeAbility, float> values = new Dictionary<HeroUpgradeAbility, float>();
    static readonly Dictionary<(AttackType, DefenceType), float> versus = new Dictionary<(AttackType, DefenceType), float>();
    static float eliteRageUntil; // KingHitEliteRage 끝나는 시각 (Time.time)
    static bool lastStandUsed;
    static bool lowHpFreezeUsed;
    public static void ResetOncePerRunForTest() { lastStandUsed = false; lowHpFreezeUsed = false; }
    // 장착 5종 조건 (게임 시작 때 한 번 판정)
    public static bool FormVarietyActive { get; private set; }
    public static bool PureTypeActive { get; private set; }

    public static float Get(HeroUpgradeAbility ability) => values.TryGetValue(ability, out float v) ? v : 0f;

    // N% 확률 판정 (값이 0이면 false)
    public static bool Roll(HeroUpgradeAbility ability) => Roll(Get(ability), Random.value);
    public static bool Roll(float percent, float random01) => percent > 0f && random01 * 100f < percent;

    public static void Begin()
    {
        values.Clear();
        versus.Clear();
        lastStandUsed = false;
        lowHpFreezeUsed = false;
        eliteRageUntil = 0f;
        var user = UserDataManager.Instance != null ? UserDataManager.Instance.userKingSlimeManager : null;
        if (user != null)
        {
            foreach (HeroUpgradeAbility a in System.Enum.GetValues(typeof(HeroUpgradeAbility)))
            {
                float v = user.GetAbilityValue(a);
                if (v > 0f && a != HeroUpgradeAbility.AttackVsDefence)
                    values[a] = v;
            }
            foreach (AttackType at in System.Enum.GetValues(typeof(AttackType)))
                foreach (DefenceType dt in System.Enum.GetValues(typeof(DefenceType)))
                {
                    float v = user.GetVersusValue(at, dt);
                    if (v > 0f)
                        versus[(at, dt)] = v;
                }
        }
        EliteArea.AnyRefreshed -= OnBuffAreaRefreshed;
        EliteArea.AnyRefreshed += OnBuffAreaRefreshed;
        RefreshEquipConditions();
        ApplyStartEffects();
        if (values.Count > 0)
            Debug.Log($"[HeroUpgrade] {string.Join(", ", values.Select(kv => $"{kv.Key} {kv.Value}"))}");
    }

    public static void End()
    {
        EliteArea.AnyRefreshed -= OnBuffAreaRefreshed;
        values.Clear();
        versus.Clear();
        eliteRageUntil = 0f;
        FormVarietyActive = false;
        PureTypeActive = false;
    }

    // 테스트용: 값 직접 지정
    public static void SetForTest(HeroUpgradeAbility ability, float value)
    {
        if (value > 0f) values[ability] = value;
        else values.Remove(ability);
    }

    static void RefreshEquipConditions()
    {
        var equipped = UserDataManager.Instance != null
            ? UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions
                .Where(m => m != null).Select(m => MinionManager.Instance.GetMinionData(m.key)).Where(d => d != null).ToList()
            : new List<MinionData>();
        FormVarietyActive = IsFormVariety(equipped.Select(d => d.tags).ToList());
        PureTypeActive = IsPureType(equipped.Select(d => d.attackType).ToList());
    }

    // 장착 미니언 형태가 모두 다름 (형태가 하나씩 있어야 함, 5종 미만이면 false)
    public static bool IsFormVariety(List<MinionTag> tags)
    {
        if (tags.Count < GameSetting.MINION_SLOT_COUNT)
            return false;
        var forms = tags.Select(MinionTags.GetForm).ToList();
        return forms.All(f => f != MinionTag.None) && forms.Distinct().Count() == forms.Count;
    }

    // 장착 미니언 공격 타입이 모두 같음 (5종 미만이면 false)
    public static bool IsPureType(List<AttackType> types) =>
        types.Count >= GameSetting.MINION_SLOT_COUNT && types.Distinct().Count() == 1;

    public static void SetVersusForTest(AttackType attackType, DefenceType defenceType, float value)
    {
        if (value > 0f) versus[(attackType, defenceType)] = value;
        else versus.Remove((attackType, defenceType));
    }

    // 테스트용: 장착 조건 직접 지정
    public static void SetConditionsForTest(bool formVariety, bool pureType)
    {
        FormVarietyActive = formVariety;
        PureTypeActive = pureType;
    }

    // ---- 시작 효과 (Begin에서 한 번) ----
    public static void ApplyStartEffects()
    {
        KingSlime king = KingSlime.Instance;
        if (king == null)
            return;
        int dice = Mathf.RoundToInt(Get(HeroUpgradeAbility.StartDiceCoin));
        if (dice > 0)
            king.bonusDiceCoinNextBreak += dice; // 첫 브레이크타임에 받음

        int unblock = Mathf.RoundToInt(Get(HeroUpgradeAbility.StartUnblockTile));
        for (int i = 0; i < unblock && TileManager.Instance != null; i++)
        {
            if (!TileManager.Instance.UnblockRandomTile())
                break;
        }

        if (Roll(HeroUpgradeAbility.StartRuneChance))
            king.pendingRuneRewards++; // 첫 브레이크타임에 룬 선택 창 (RuneRewardSystem)
    }

    // ---- 카드 ----
    // Lv.2 카드 확률에 더하는 % (첫 브레이크타임 첫 지급이면 FirstOfferLv2Chance도)
    public static float GetOfferLv2Bonus(bool firstDeal) =>
        Get(HeroUpgradeAbility.OfferLv2Chance) + (firstDeal ? Get(HeroUpgradeAbility.FirstOfferLv2Chance) : 0f);

    // ---- 처치 ----
    // 엘리트·보스 처치 (보너스 엘리트 제외, RuneRewardSystem.AddReward): 확률로 다음 브레이크타임 주사위 코인 +1
    public static void OnEliteBossKilled(Vector3 pos)
    {
        KingSlime king = KingSlime.Instance;
        if (king == null || !Roll(HeroUpgradeAbility.EliteBossDiceChance))
            return;
        king.AddRewardDiceCoin(1);
        EffectManager.PlayPopText((Vector2)pos + Vector2.up * 0.5f, TranslateManager.GetText("HeroUpgrade_DiceNextBreak"), new Color(1f, 0.85f, 0.3f));
    }

    // 경험치 구매 시 더 받는 경험치 (확률)
    public static int RollExpBuyBonus() => Roll(HeroUpgradeAbility.ExpBuyBonusChance) ? 1 : 0;

    // ---- 킹 체력 ----
    // 체력이 0이 됐을 때 버티면 버틴 체력, 아니면 0 (판당 1번)
    public static float TryLastStand(float maxHp)
    {
        float percent = Get(HeroUpgradeAbility.LastStand);
        if (lastStandUsed || percent <= 0f)
            return 0f;
        lastStandUsed = true;
        return Mathf.Max(1f, maxHp * percent / 100f);
    }

    // 피해 후 체력이 처음 LOW_HP_RATIO 아래가 되면 모든 적 정지 (판당 1번)
    public static void CheckLowHp(float curHp, float maxHp)
    {
        float seconds = Get(HeroUpgradeAbility.LowHpFreeze);
        if (lowHpFreezeUsed || seconds <= 0f || curHp <= 0f || curHp >= maxHp * LOW_HP_RATIO)
            return;
        lowHpFreezeUsed = true;
        Enemy.FreezeAll(seconds);
        if (KingSlime.Instance != null)
            EffectManager.PlayPopText((Vector2)KingSlime.Instance.transform.position + Vector2.up, TranslateManager.GetText("HeroUpgrade_Freeze"), new Color(0.6f, 0.85f, 1f));
    }

    // ---- 킹 피해 (KingSlimeHealth, 실제로 줄어든 체력) ----
    public static void OnKingDamaged(float lostHp)
    {
        if (lostHp <= 0f)
            return;
        float charge = Get(HeroUpgradeAbility.KingHitEnergyCharge);
        if (charge > 0f && KingSlime.Instance != null)
            KingSlime.Instance.abilityInventory?.Get<HeroKingAbility>()?.AddChargeTime(charge * lostHp);
        if (Get(HeroUpgradeAbility.KingHitEliteRage) > 0f)
        {
            bool wasOn = IsEliteRageOn;
            eliteRageUntil = Time.time + ELITE_RAGE_DURATION;
            if (!wasOn)
            {
                RefreshFieldMinions();
                EndEliteRageLater().Forget();
            }
        }
    }

    public static bool IsEliteRageOn => Time.time < eliteRageUntil;

    static async UniTaskVoid EndEliteRageLater()
    {
        while (IsEliteRageOn)
            await UniTask.Yield();
        RefreshFieldMinions();
    }

    // ---- 미니언 ----
    // 정예병 칸 = 용사(EliteArea) 버프 칸. 칸이 다시 계산되면 미니언 스탯도 갱신
    static void OnBuffAreaRefreshed()
    {
        if (Get(HeroUpgradeAbility.EliteMinionAttack) > 0f || Get(HeroUpgradeAbility.EliteMinionAttackSpeed) > 0f
            || Get(HeroUpgradeAbility.EliteMinionSkillCooldown) > 0f || IsEliteRageOn)
            RefreshFieldMinions();
    }

    static bool refreshing;
    static void RefreshFieldMinions()
    {
        if (refreshing || KingSlime.Instance == null || KingSlime.Instance.minionInventory == null)
            return;
        refreshing = true;
        try
        {
            foreach (Minion m in KingSlime.Instance.minionInventory.curMinions.ToList())
                if (m != null && m.minionData != null && m.isActiveAndEnabled)
                    m.UpdateMinion();
        }
        finally { refreshing = false; }
    }

    public static bool IsEliteMinion(Minion minion) =>
        TileManager.Instance != null && EliteArea.IsEliteTile(TileManager.Instance.GetTile(minion));

    // 근거리 = 근접(Minion.IsMelee, 버서커) 또는 회전형 / 원거리 = 발사형 중 근거리가 아닌 것
    public static bool IsMelee(Minion minion) =>
        minion.IsMelee || (minion.minionData != null && MinionTags.GetForm(minion.minionData.tags) == MinionTag.Orbit);
    public static bool IsRanged(Minion minion) =>
        !IsMelee(minion) && minion.minionData != null && MinionTags.GetForm(minion.minionData.tags) == MinionTag.Shooter;

    // 스킬 쿨타임 충전 +% (정예병 칸)
    public static float GetMinionSkillCooldownAdd(Minion minion)
    {
        float v = Get(HeroUpgradeAbility.EliteMinionSkillCooldown);
        return v > 0f && IsEliteMinion(minion) ? v : 0f;
    }

    // 공격력 배율 (같은 종 이웃 + 5성 진화 + 정예병 + 근거리 + 마법 + 킹 피격 분노)
    public static float GetMinionAttackMul(Minion minion)
    {
        float mul = 1f;
        bool elite = IsEliteMinion(minion);
        float eliteAtk = Get(HeroUpgradeAbility.EliteMinionAttack);
        if (eliteAtk > 0f && elite)
            mul *= 1f + eliteAtk / 100f;
        float rage = Get(HeroUpgradeAbility.KingHitEliteRage);
        if (rage > 0f && elite && IsEliteRageOn)
            mul *= 1f + rage / 100f;
        float melee = Get(HeroUpgradeAbility.MeleeAttack);
        if (melee > 0f && IsMelee(minion))
            mul *= 1f + melee / 100f;
        float magic = Get(HeroUpgradeAbility.MagicAttack);
        if (magic > 0f && minion.minionData != null && minion.minionData.attackType == AttackType.Magic)
            mul *= 1f + magic / 100f;
        float twin = Get(HeroUpgradeAbility.TwinAttack);
        if (twin > 0f && HasAdjacentSameKey(minion))
            mul *= 1f + twin / 100f;
        float evolved = Get(HeroUpgradeAbility.EvolvedAttack);
        if (evolved > 0f && minion.evolution != null)
            mul *= 1f + evolved / 100f;
        return mul;
    }

    // 공속 배율 (장착 형태가 모두 다름 + 정예병 + 원거리)
    public static float GetMinionAttackSpeedMul(Minion minion)
    {
        float mul = 1f;
        float v = Get(HeroUpgradeAbility.FormVarietyAttackSpeed);
        if (FormVarietyActive && v > 0f)
            mul *= 1f + v / 100f;
        float elite = Get(HeroUpgradeAbility.EliteMinionAttackSpeed);
        if (elite > 0f && IsEliteMinion(minion))
            mul *= 1f + elite / 100f;
        float ranged = Get(HeroUpgradeAbility.RangedAttackSpeed);
        if (ranged > 0f && IsRanged(minion))
            mul *= 1f + ranged / 100f;
        return mul;
    }

    // ---- 룬 보상 ----
    // 룬 보상(엘리트·보스)으로 새기거나 올린 룬: 확률로 1레벨 더 (최대 레벨이면 그대로)
    public static void OnRuneRewardPlaced(TileRune rune, Vector3 pos)
    {
        if (rune == null || !Roll(HeroUpgradeAbility.RuneRewardLv2Chance))
            return;
        if (rune.ForceUpgrade())
            EffectManager.PlayPopText((Vector2)pos + Vector2.up * 0.5f, $"Lv.{rune.upgradeLevel}!", new Color(1f, 0.85f, 0.25f));
    }

    // 약점 배율 (>1)에만 곱함 (장착 공격 타입이 모두 같음)
    public static float ApplyWeakness(float multiplier)
    {
        float v = Get(HeroUpgradeAbility.PureTypeWeakness);
        return PureTypeActive && v > 0f && multiplier > 1f ? multiplier * (1f + v / 100f) : multiplier;
    }

    // 아군 공격의 방어 타입 배율 보정 (EnemyDefence): 약점(PureType) → 내성 완화(1 미만이면 +N%p, 최대 1) → 공격 타입별 방어 타입 상대 피해
    public static float ApplyDefenceMultiplier(float multiplier, AttackType attackType, DefenceType defenceType)
    {
        multiplier = ApplyWeakness(multiplier);
        multiplier = ApplyResistEase(multiplier, Get(HeroUpgradeAbility.ResistEase));
        if (versus.TryGetValue((attackType, defenceType), out float v))
            multiplier *= 1f + v / 100f;
        return multiplier;
    }

    public static float ApplyResistEase(float multiplier, float percent) =>
        percent > 0f && multiplier < 1f ? Mathf.Min(1f, multiplier + percent / 100f) : multiplier;

    static readonly Vector2Int[] CROSS_OFFSETS = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    // 같은 TileGroup 상하좌우 칸에 같은 종 미니언이 있음
    public static bool HasAdjacentSameKey(Minion minion)
    {
        if (TileManager.Instance == null || minion == null)
            return false;
        Tile center = TileManager.Instance.GetTile(minion);
        if (center == null)
            return false;
        foreach (Tile tile in TileManager.Instance.GetTilesAround(center, CROSS_OFFSETS))
        {
            if (tile.tileGroup == center.tileGroup && tile.tileObject is Minion other && other != minion && other.key == minion.key)
                return true;
        }
        return false;
    }
}

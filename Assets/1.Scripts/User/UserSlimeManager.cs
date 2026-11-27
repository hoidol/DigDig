using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// (2026-11-24) 용사 강화 포인트 (코드 이름 SlimeLevelStat): 유저 레벨 1당 1포인트 → 묶음(SlimeLevelStatSetting.Tier)의 스탯에 찍음, 언제든 무료 초기화
// (이전: 재화로 StatType별 영구 업그레이드 KSUpgradeData → 삭제. 저장 데이터 마이그레이션 안 함)
[System.Serializable]
public class UserSlimeManager : UserBaseManager
{
    public const string UserDataFileName = "UserSlimeData";

    [field: SerializeField]
    public UserKSData userKSData
    {
        get; private set;
    }

    public override void LoadData()
    {
        userKSData = SaveManager.LoadData<UserKSData>(UserDataFileName);
        if (userKSData == null)
            userKSData = new UserKSData();
        if (userKSData.statPoints == null)
            userKSData.statPoints = new List<UserKSStatPoint>();

        RequestSave();
    }

    static int UserLv => UserDataManager.Instance.userMyInfoManager.userMyInfoData.lv;

    #region 포인트 (씬 없이 계산하는 규칙은 static)
    // (2026-11-26) 포인트는 묶음마다 따로: 묶음 = 레벨 구간 [unlockLevel, unlockLevel + maxPoints - 1] (Lv.1~5 → 1번 묶음, Lv.6~10 → 2번 …)
    // 그 구간 레벨에서 얻은 포인트는 그 묶음 칸 3개에만 나눠 찍음 (이전: 전체 포인트를 Lv.5·10…에 열린 묶음 어디에나)
    public static int GetTierEarnedPoints(int userLv, int unlockLevel, int maxPoints) =>
        Mathf.Clamp(userLv - unlockLevel + 1, 0, Mathf.Max(0, maxPoints)) * SlimeLevelStatSetting.POINTS_PER_LEVEL;
    public static int GetTierEarnedPoints(int userLv, SlimeLevelStatSetting.Tier tier) =>
        tier == null ? 0 : GetTierEarnedPoints(userLv, tier.unlockLevel, tier.maxPoints);
    public static int GetTotalPoints(int userLv) => SlimeLevelStatSetting.Tiers.Sum(t => GetTierEarnedPoints(userLv, t));

    public int TotalPoints => GetTotalPoints(UserLv);
    public int UsedPoints => userKSData.statPoints.Sum(p => p.points);
    public int RemainPoints
    {
        get
        {
            var tiers = SlimeLevelStatSetting.Tiers;
            int sum = 0;
            for (int i = 0; i < tiers.Length; i++)
                sum += GetTierRemainPoints(i);
            return sum;
        }
    }

    public int GetPoints(int tierIdx, int optionIdx) =>
        userKSData.statPoints.FirstOrDefault(p => p.tierIdx == tierIdx && p.optionIdx == optionIdx)?.points ?? 0;

    public int GetTierPoints(int tierIdx) =>
        userKSData.statPoints.Where(p => p.tierIdx == tierIdx).Sum(p => p.points);
    public int GetTierEarnedPoints(int tierIdx) => GetTierEarnedPoints(UserLv, GetTier(tierIdx));
    public int GetTierRemainPoints(int tierIdx) => Mathf.Max(0, GetTierEarnedPoints(tierIdx) - GetTierPoints(tierIdx));
    // 이 묶음의 다음 포인트를 받는 레벨 (다 받았으면 -1)
    public int GetTierNextPointLevel(int tierIdx)
    {
        var tier = GetTier(tierIdx);
        if (tier == null)
            return -1;
        int earnedLevels = GetTierEarnedPoints(tierIdx) / Mathf.Max(1, SlimeLevelStatSetting.POINTS_PER_LEVEL);
        return earnedLevels >= tier.maxPoints ? -1 : tier.unlockLevel + earnedLevels;
    }

    public static bool IsTierUnlocked(int userLv, SlimeLevelStatSetting.Tier tier) => tier != null && userLv >= tier.unlockLevel;
    public bool IsTierUnlocked(int tierIdx) => IsTierUnlocked(UserLv, GetTier(tierIdx));

    static SlimeLevelStatSetting.Tier GetTier(int tierIdx)
    {
        var tiers = SlimeLevelStatSetting.Tiers;
        return tierIdx >= 0 && tierIdx < tiers.Length ? tiers[tierIdx] : null;
    }

    // 1포인트를 찍을 수 있는지 (잠김 → 묶음 상한 → 이 묶음에서 받은 포인트를 다 씀 순으로 판정)
    public static SlimeStatPointResult CanAddPoint(int userLv, int unlockLevel, int tierUsed, int maxPoints)
    {
        if (userLv < unlockLevel)
            return SlimeStatPointResult.Locked;
        if (tierUsed >= maxPoints)
            return SlimeStatPointResult.TierFull;
        if (tierUsed >= GetTierEarnedPoints(userLv, unlockLevel, maxPoints))
            return SlimeStatPointResult.NoPoint;
        return SlimeStatPointResult.Success;
    }

    public SlimeStatPointResult TryAddPoint(int tierIdx, int optionIdx)
    {
        var tier = GetTier(tierIdx);
        if (tier == null || optionIdx < 0 || optionIdx >= tier.options.Length)
            return SlimeStatPointResult.Locked;
        SlimeStatPointResult result = CanAddPoint(UserLv, tier.unlockLevel, GetTierPoints(tierIdx), tier.maxPoints);
        if (result != SlimeStatPointResult.Success)
            return result;

        UserKSStatPoint point = userKSData.statPoints.FirstOrDefault(p => p.tierIdx == tierIdx && p.optionIdx == optionIdx);
        if (point == null)
        {
            point = new UserKSStatPoint { tierIdx = tierIdx, optionIdx = optionIdx };
            userKSData.statPoints.Add(point);
        }
        point.points++;
        RequestSave();
        GameEventBus.Publish(new SlimeStatPointChangedEvent());
        return result;
    }

    // 전체 초기화 (무료)
    public void ResetPoints()
    {
        if (userKSData.statPoints.Count == 0)
            return;
        userKSData.statPoints.Clear();
        RequestSave();
        GameEventBus.Publish(new SlimeStatPointChangedEvent());
    }
    #endregion

    // 기본값에 찍은 포인트를 반영한 값: 기본 × (1 + 곱연산 %합 / 100) + 더하기 합
    // 잠긴 묶음(설정이 바뀌어 레벨이 모자라게 된 경우)의 포인트는 반영하지 않음
    public float ApplyStatPoints(StatType statType, float baseValue)
    {
        float add = 0f, mulPercent = 0f;
        var tiers = SlimeLevelStatSetting.Tiers;
        foreach (UserKSStatPoint p in userKSData.statPoints)
        {
            if (p.points <= 0 || p.tierIdx < 0 || p.tierIdx >= tiers.Length)
                continue;
            var tier = tiers[p.tierIdx];
            if (!IsTierUnlocked(UserLv, tier) || p.optionIdx < 0 || p.optionIdx >= tier.options.Length)
                continue;
            var option = tier.options[p.optionIdx];
            if (option.kind != SlimeLevelStatSetting.OptionKind.Stat || option.statType != statType)
                continue;
            if (option.multiply)
                mulPercent += option.ValueAt(p.points);
            else
                add += option.ValueAt(p.points);
        }
        return CalcStat(baseValue, add, mulPercent);
    }

    // 용사 강화 능력 값 (찍은 포인트의 값, 여러 칸에 같은 능력이 있으면 합). 안 찍었거나 잠긴 묶음이면 0
    public float GetAbilityValue(HeroUpgradeAbility ability)
    {
        float sum = 0f;
        var tiers = SlimeLevelStatSetting.Tiers;
        foreach (UserKSStatPoint p in userKSData.statPoints)
        {
            if (p.points <= 0 || p.tierIdx < 0 || p.tierIdx >= tiers.Length)
                continue;
            var tier = tiers[p.tierIdx];
            if (!IsTierUnlocked(UserLv, tier) || p.optionIdx < 0 || p.optionIdx >= tier.options.Length)
                continue;
            var option = tier.options[p.optionIdx];
            if (option.kind == SlimeLevelStatSetting.OptionKind.Ability && option.ability == ability)
                sum += option.ValueAt(p.points);
        }
        return sum;
    }

    // 공격 타입 → 방어 타입 상대 피해 % (AttackVsDefence 칸들의 합)
    public float GetVersusValue(AttackType attackType, DefenceType defenceType)
    {
        float sum = 0f;
        var tiers = SlimeLevelStatSetting.Tiers;
        foreach (UserKSStatPoint p in userKSData.statPoints)
        {
            if (p.points <= 0 || p.tierIdx < 0 || p.tierIdx >= tiers.Length)
                continue;
            var tier = tiers[p.tierIdx];
            if (!IsTierUnlocked(UserLv, tier) || p.optionIdx < 0 || p.optionIdx >= tier.options.Length)
                continue;
            var o = tier.options[p.optionIdx];
            if (o.kind == SlimeLevelStatSetting.OptionKind.Ability && o.ability == HeroUpgradeAbility.AttackVsDefence
                && o.attackType == attackType && o.defenceType == defenceType)
                sum += o.ValueAt(p.points);
        }
        return sum;
    }

    public static float CalcStat(float baseValue, float add, float mulPercent) => baseValue * (1f + mulPercent / 100f) + add;

    public override void SaveData()
    {
        SaveManager.SaveData(UserDataFileName, userKSData);
    }
}

public enum SlimeStatPointResult
{
    Success,
    Locked,   // 묶음 해금 레벨 미달
    TierFull, // 묶음 포인트 상한
    NoPoint,  // 이 묶음에서 받은 포인트를 다 씀 (다음 포인트는 다음 레벨)
}

[System.Serializable]
public class UserKSData
{
    public List<UserKSStatPoint> statPoints = new List<UserKSStatPoint>();
}

// 묶음 index(SlimeLevelStatSetting.tiers) + 그 안의 스탯 index에 찍은 포인트
[System.Serializable]
public class UserKSStatPoint
{
    public int tierIdx;
    public int optionIdx;
    public int points;
}

public class SlimeStatPointChangedEvent { }

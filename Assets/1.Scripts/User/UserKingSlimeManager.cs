using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// (2026-11-24) 용사 강화 포인트 (코드 이름 KingLevelStat): 유저 레벨 1당 1포인트 → 묶음(KingLevelStatSetting.Tier)의 스탯에 찍음, 언제든 무료 초기화
// (이전: 재화로 StatType별 영구 업그레이드 KSUpgradeData → 삭제. 저장 데이터 마이그레이션 안 함)
[System.Serializable]
public class UserKingSlimeManager : UserBaseManager
{
    public const string UserDataFileName = "UserKingSlimeData";

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
    public static int GetTotalPoints(int userLv) => Mathf.Max(0, userLv) * KingLevelStatSetting.POINTS_PER_LEVEL;

    public int TotalPoints => GetTotalPoints(UserLv);
    public int UsedPoints => userKSData.statPoints.Sum(p => p.points);
    public int RemainPoints => Mathf.Max(0, TotalPoints - UsedPoints);

    public int GetPoints(int tierIdx, int optionIdx) =>
        userKSData.statPoints.FirstOrDefault(p => p.tierIdx == tierIdx && p.optionIdx == optionIdx)?.points ?? 0;

    public int GetTierPoints(int tierIdx) =>
        userKSData.statPoints.Where(p => p.tierIdx == tierIdx).Sum(p => p.points);

    public static bool IsTierUnlocked(int userLv, KingLevelStatSetting.Tier tier) => tier != null && userLv >= tier.unlockLevel;
    public bool IsTierUnlocked(int tierIdx) => IsTierUnlocked(UserLv, GetTier(tierIdx));

    static KingLevelStatSetting.Tier GetTier(int tierIdx)
    {
        var tiers = KingLevelStatSetting.Tiers;
        return tierIdx >= 0 && tierIdx < tiers.Length ? tiers[tierIdx] : null;
    }

    // 1포인트를 찍을 수 있는지 (잠김 → 묶음 상한 → 남은 포인트 순으로 판정)
    public static KingStatPointResult CanAddPoint(int userLv, int unlockLevel, int tierUsed, int maxPoints, int remain)
    {
        if (userLv < unlockLevel)
            return KingStatPointResult.Locked;
        if (tierUsed >= maxPoints)
            return KingStatPointResult.TierFull;
        if (remain <= 0)
            return KingStatPointResult.NoPoint;
        return KingStatPointResult.Success;
    }

    public KingStatPointResult TryAddPoint(int tierIdx, int optionIdx)
    {
        var tier = GetTier(tierIdx);
        if (tier == null || optionIdx < 0 || optionIdx >= tier.options.Length)
            return KingStatPointResult.Locked;
        KingStatPointResult result = CanAddPoint(UserLv, tier.unlockLevel, GetTierPoints(tierIdx), tier.maxPoints, RemainPoints);
        if (result != KingStatPointResult.Success)
            return result;

        UserKSStatPoint point = userKSData.statPoints.FirstOrDefault(p => p.tierIdx == tierIdx && p.optionIdx == optionIdx);
        if (point == null)
        {
            point = new UserKSStatPoint { tierIdx = tierIdx, optionIdx = optionIdx };
            userKSData.statPoints.Add(point);
        }
        point.points++;
        RequestSave();
        GameEventBus.Publish(new KingStatPointChangedEvent());
        return result;
    }

    // 전체 초기화 (무료)
    public void ResetPoints()
    {
        if (userKSData.statPoints.Count == 0)
            return;
        userKSData.statPoints.Clear();
        RequestSave();
        GameEventBus.Publish(new KingStatPointChangedEvent());
    }
    #endregion

    // 기본값에 찍은 포인트를 반영한 값: 기본 × (1 + 곱연산 %합 / 100) + 더하기 합
    // 잠긴 묶음(설정이 바뀌어 레벨이 모자라게 된 경우)의 포인트는 반영하지 않음
    public float ApplyStatPoints(StatType statType, float baseValue)
    {
        float add = 0f, mulPercent = 0f;
        var tiers = KingLevelStatSetting.Tiers;
        foreach (UserKSStatPoint p in userKSData.statPoints)
        {
            if (p.points <= 0 || p.tierIdx < 0 || p.tierIdx >= tiers.Length)
                continue;
            var tier = tiers[p.tierIdx];
            if (!IsTierUnlocked(UserLv, tier) || p.optionIdx < 0 || p.optionIdx >= tier.options.Length)
                continue;
            var option = tier.options[p.optionIdx];
            if (option.kind != KingLevelStatSetting.OptionKind.Stat || option.statType != statType)
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
        var tiers = KingLevelStatSetting.Tiers;
        foreach (UserKSStatPoint p in userKSData.statPoints)
        {
            if (p.points <= 0 || p.tierIdx < 0 || p.tierIdx >= tiers.Length)
                continue;
            var tier = tiers[p.tierIdx];
            if (!IsTierUnlocked(UserLv, tier) || p.optionIdx < 0 || p.optionIdx >= tier.options.Length)
                continue;
            var option = tier.options[p.optionIdx];
            if (option.kind == KingLevelStatSetting.OptionKind.Ability && option.ability == ability)
                sum += option.ValueAt(p.points);
        }
        return sum;
    }

    // 공격 타입 → 방어 타입 상대 피해 % (AttackVsDefence 칸들의 합)
    public float GetVersusValue(AttackType attackType, DefenceType defenceType)
    {
        float sum = 0f;
        var tiers = KingLevelStatSetting.Tiers;
        foreach (UserKSStatPoint p in userKSData.statPoints)
        {
            if (p.points <= 0 || p.tierIdx < 0 || p.tierIdx >= tiers.Length)
                continue;
            var tier = tiers[p.tierIdx];
            if (!IsTierUnlocked(UserLv, tier) || p.optionIdx < 0 || p.optionIdx >= tier.options.Length)
                continue;
            var o = tier.options[p.optionIdx];
            if (o.kind == KingLevelStatSetting.OptionKind.Ability && o.ability == HeroUpgradeAbility.AttackVsDefence
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

public enum KingStatPointResult
{
    Success,
    Locked,   // 묶음 해금 레벨 미달
    TierFull, // 묶음 포인트 상한
    NoPoint,  // 남은 포인트 없음
}

[System.Serializable]
public class UserKSData
{
    public List<UserKSStatPoint> statPoints = new List<UserKSStatPoint>();
}

// 묶음 index(KingLevelStatSetting.tiers) + 그 안의 스탯 index에 찍은 포인트
[System.Serializable]
public class UserKSStatPoint
{
    public int tierIdx;
    public int optionIdx;
    public int points;
}

public class KingStatPointChangedEvent { }

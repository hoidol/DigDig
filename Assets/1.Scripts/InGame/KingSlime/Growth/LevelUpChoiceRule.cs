using System;
using System.Collections.Generic;
using UnityEngine;

// (2026-11-20) 레벨업 카드 구성 규칙 (씬 없이 계산 → 테스트 가능)
// (2026-11-26 최신) 최대 Lv.12. 레벨업 한 번 = [Lv.1만 용사 선택] → 스탯 변화 화면(스킬 해방 Lv.2·6·10은 여기서 자동, Lv.7 미니언 수 +1)
//   → [용사 강화 Lv.3·4·5·7·8·9·11·12] → 미니언 강화 (LevelUpCanvas). 정예 칸 추가·충전 가속 최대 5, 에너지 보유 최대 3
// (이전 2026-11-24) 최대 Lv.8. 레벨업 한 번 = ① 킹 능력치 카드(LevelUpCanvas, 매 레벨 유지) → ② 이 규칙의 단계 카드
//   Lv.1 용사 선택(정예병 칸 랜덤 2칸) → Lv.2 1단계 스킬 해방 → Lv.3·4 용사 강화 → Lv.5 2단계 스킬 해방(+최대 미니언 수 +1)
//   → Lv.6·7 용사 강화 → Lv.8 3단계 스킬 해방 (공용 "용사가 될거야!")
// - (2026-11-25) 용사 강화 = 정예 칸 추가(EliteAreaExpandKingAbility, 카드마다 다른 랜덤 칸, 최대 3) · 충전 가속(-10%/Lv, 최대 3)
//   · 에너지 보유 증가(2 → 3 → 4 → 5, 최대 3) · 1단계 스킬 강화(1단계 해방 후) · 2단계 스킬 강화(2단계 해방 후) (각 최대 1)
//   카드 3장 = 정예 칸 추가가 아닌 카드 1장씩 + 남은 자리는 정예 칸 추가(서로 다른 칸)로 채움 (GetChoices HeroBoost)
//   정예병 버프 강화(용사 카드 레벨 +1)는 삭제 → 정예병 칸 버프·킹 스탯은 레벨업마다 자동 상승 (HeroKingAbility.HeroLevel)
// - 앞 단계를 아직 안 했으면 그 단계가 먼저 (레벨을 건너뛰어도 순서대로)
// - level = 그 레벨업 창의 레벨 (밀린 레벨업은 하나씩: KingSlime.ChoiceLevel)
public static class LevelUpChoiceRule
{
    public const int CHOICE_COUNT = 3;
    public const int MAX_LEVEL = 12;
    public const int HERO_LEVEL = 1;
    public const int HERO_TILE_COUNT = 2; // 용사 정예병 칸 수

    // 레벨마다 단계 (순서 = 진행 순서). 같은 종류가 여러 번이면 그 횟수만큼 해야 다음으로
    public static readonly (int level, AbilityCategory stage)[] MILESTONES =
    {
        (1, AbilityCategory.Hero),
        (2, AbilityCategory.HeroSkill),
        (3, AbilityCategory.HeroBoost),
        (4, AbilityCategory.HeroBoost),
        (5, AbilityCategory.HeroBoost),
        (6, AbilityCategory.HeroSkill),
        (7, AbilityCategory.HeroBoost),
        (8, AbilityCategory.HeroBoost),
        (9, AbilityCategory.HeroBoost),
        (10, AbilityCategory.HeroSkill),
        (11, AbilityCategory.HeroBoost),
        (12, AbilityCategory.HeroBoost),
    };

    // (2026-11-26) 필드 최대 미니언 수 +1 받는 레벨 (용사 레벨 기준, HeroKingAbility.RefreshMinionCountBonus · 스탯 변화 화면 표시)
    public static readonly int[] MINION_COUNT_UP_LEVELS = { 7 };
    public const int MINION_COUNT_UP_AMOUNT = 1;

    public static int GetMinionCountAdd(int heroLevel)
    {
        int n = 0;
        foreach (int lv in MINION_COUNT_UP_LEVELS)
            if (heroLevel >= lv)
                n += MINION_COUNT_UP_AMOUNT;
        return n;
    }

    // 스킬 해방 레벨 (index = 스킬 단계 0·1·2). 화면 표시용
    public static int GetSkillUnlockLevel(int skillIdx)
    {
        int n = 0;
        foreach (var (lv, stage) in MILESTONES)
            if (stage == AbilityCategory.HeroSkill && n++ == skillIdx)
                return lv;
        return MAX_LEVEL;
    }

    // 레벨업 창 한 번의 카드 구성에 필요한 값
    public class Input
    {
        public int level;
        public AbilityData ownedHero;   // 고른 용사 (없으면 null)
        public int heroLevel;           // 0 = 없음, 1 = 고름
        public int heroSkillCount;      // 해방한 스킬 수 (0~3)
        public int boostCount;          // 용사 강화 단계에서 고른 수 (boostPool 카드 레벨 합)
        public IList<AbilityData> heroPool;
        public IList<AbilityData> boostPool; // 정예 칸 추가 · 충전 가속
        public Func<AbilityData, int> getLevel;
    }

    // 이번 레벨업 창의 단계 (Stat = 단계 카드 없음 → 능력치 카드만)
    public static AbilityCategory GetStage(Input input)
    {
        int skillNeed = 0, boostNeed = 0;
        foreach (var (lv, stage) in MILESTONES)
        {
            if (input.level < lv)
                break;
            switch (stage)
            {
                case AbilityCategory.Hero:
                    if (input.heroLevel < 1 && HasAny(input.heroPool)) return stage;
                    break;
                case AbilityCategory.HeroSkill:
                    skillNeed++;
                    if (input.heroLevel >= 1 && input.heroSkillCount < skillNeed) return stage;
                    break;
                case AbilityCategory.HeroBoost:
                    boostNeed++;
                    if (input.heroLevel >= 1 && input.boostCount < boostNeed && GetBoostCandidates(input).Count > 0) return stage;
                    break;
            }
        }
        return AbilityCategory.Stat;
    }

    static bool HasAny(IList<AbilityData> pool) => pool != null && pool.Count > 0;

    // 용사 강화 후보 = boostPool (최대 레벨 제외)
    public static List<AbilityData> GetBoostCandidates(Input input)
    {
        Func<AbilityData, int> getLevel = input.getLevel ?? (_ => 0);
        var list = new List<AbilityData>();
        if (input.boostPool != null)
            foreach (var d in input.boostPool)
                if (d != null && !list.Contains(d) && getLevel(d) < d.maxLevel && IsBoostUnlocked(d, input.heroSkillCount))
                    list.Add(d);
        return list;
    }

    // 스킬 강화 카드는 그 단계 스킬을 해방한 뒤부터 (1단계 = Lv.2, 2단계 = Lv.5)
    public static bool IsBoostUnlocked(AbilityData d, int heroSkillCount) =>
        !(d.prefab is HeroSkillEnhanceKingAbility s) || heroSkillCount > s.skillIndex;

    // 레벨업 창 안내·버튼 글씨 (단계 이름, 번역 전 임시)
    public static string GetStageName(AbilityCategory stage)
    {
        switch (stage)
        {
            case AbilityCategory.Hero: return "용사 선택";
            case AbilityCategory.HeroSkill: return "스킬 해방";
            case AbilityCategory.HeroBoost:
            case AbilityCategory.HeroUpgrade: return "용사 강화";
            case AbilityCategory.TileExpand: return "버프 칸 확장";
            default: return "레벨 업";
        }
    }

    // 단계 카드. 단계가 없으면 빈 목록 → 능력치 카드만 고르고 끝
    // 스킬 해방 = 고른 용사 한 장 (LevelUpCanvas가 다음 스킬 카드로 보여줌, 고르면 HeroKingAbility.UnlockNextSkill)
    public static List<AbilityData> GetChoices(Input input, System.Random rng, int count = CHOICE_COUNT)
    {
        Func<AbilityData, int> getLevel = input.getLevel ?? (_ => 0);
        switch (GetStage(input))
        {
            case AbilityCategory.Hero:
                return PickStatChoices(input.heroPool, getLevel, rng, count);
            case AbilityCategory.HeroSkill:
                return new List<AbilityData> { input.ownedHero };
            case AbilityCategory.HeroBoost:
                return PickHeroBoostChoices(GetBoostCandidates(input), rng, count);
        }
        return new List<AbilityData>();
    }

    // (2026-11-25) 용사 강화 카드: 정예 칸 추가(TileExpand)가 아닌 후보는 1장씩, 남은 자리 = 정예 칸 추가 (같은 카드 여러 장 → LevelUpCanvas가 장마다 다른 칸)
    public static List<AbilityData> PickHeroBoostChoices(IList<AbilityData> candidates, System.Random rng, int count = CHOICE_COUNT)
    {
        var result = new List<AbilityData>();
        AbilityData expand = null;
        var others = new List<AbilityData>();
        foreach (var d in candidates)
        {
            if (d == null)
                continue;
            if (d.category == AbilityCategory.TileExpand)
                expand = expand ?? d;
            else if (!others.Contains(d))
                others.Add(d);
        }
        while (others.Count > 0 && result.Count < count - (expand != null ? 1 : 0))
        {
            int idx = rng.Next(others.Count);
            result.Add(others[idx]);
            others.RemoveAt(idx);
        }
        if (expand != null)
            while (result.Count < count)
                result.Insert(rng.Next(result.Count + 1), expand);
        return result;
    }

    // 용사 카드마다 정예병 칸: 킹 주변 8칸 중 서로 다른 랜덤 count칸
    public static Vector2Int[] PickHeroTiles(System.Random rng, int count = HERO_TILE_COUNT)
    {
        var pool = new List<Vector2Int>(AROUND_OFFSETS);
        var result = new List<Vector2Int>();
        while (result.Count < count && pool.Count > 0)
        {
            int idx = rng.Next(pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return result.ToArray();
    }

    // 킹 주변 8칸 (Tile.index 단위, y 아래 +)
    public static readonly Vector2Int[] AROUND_OFFSETS =
    {
        new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
        new Vector2Int(-1, 0), new Vector2Int(1, 0),
        new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1),
    };

    // 버프 칸 확장 카드마다 추가 칸: 지금 버프 칸(종족 칸)을 뺀 나머지 칸 중 랜덤, 카드끼리 겹치지 않게 (남은 칸이 모자라면 겹침 허용)
    public static List<Vector2Int> PickExpandTiles(IList<Vector2Int> usedOffsets, int count, System.Random rng)
    {
        var free = new List<Vector2Int>();
        foreach (var o in AROUND_OFFSETS)
            if (usedOffsets == null || !usedOffsets.Contains(o))
                free.Add(o);
        var result = new List<Vector2Int>();
        if (free.Count == 0)
            return result;
        var pool = new List<Vector2Int>(free);
        while (result.Count < count)
        {
            if (pool.Count == 0)
                pool.AddRange(free);
            int idx = rng.Next(pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return result;
    }

    // pool에서 최대 레벨이 아닌 서로 다른 카드를 랜덤으로 count장
    public static List<AbilityData> PickStatChoices(IList<AbilityData> pool, Func<AbilityData, int> getLevel, System.Random rng, int count = CHOICE_COUNT)
    {
        var candidates = new List<AbilityData>();
        if (pool != null)
        {
            foreach (var data in pool)
            {
                if (data == null || candidates.Contains(data))
                    continue;
                if (getLevel(data) >= data.maxLevel)
                    continue;
                candidates.Add(data);
            }
        }

        var result = new List<AbilityData>();
        while (result.Count < count && candidates.Count > 0)
        {
            int idx = rng.Next(candidates.Count);
            result.Add(candidates[idx]);
            candidates.RemoveAt(idx);
        }
        return result;
    }
}

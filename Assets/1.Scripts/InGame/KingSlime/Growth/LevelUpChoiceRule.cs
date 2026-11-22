using System;
using System.Collections.Generic;
using UnityEngine;

// (2026-11-20) 레벨업 카드 구성 규칙 (씬 없이 계산 → 테스트 가능)
// (2026-11-21) 단계 = Lv.1 종족 선택 → Lv.5 종족 강화(버프 칸 확장) → Lv.10 직업 선택 → Lv.15 패시브 → (Lv.20 발동 스킬, 아직 없음)
// - 앞 단계를 아직 안 했으면 그 단계가 먼저 (레벨을 건너뛰어도 순서대로)
// - 그 외 = 킹 능력치 카드 (최대 레벨 제외, 서로 다른 카드 count장). 고른 패시브가 최대 레벨이 아니면 강화 카드로 같이 섞임
// - level = 그 레벨업 창의 레벨 (밀린 레벨업은 하나씩: KingSlime.ChoiceLevel)
public static class LevelUpChoiceRule
{
    public const int CHOICE_COUNT = 3;
    public const int RACE_LEVEL = 1;
    public const int TILE_EXPAND_LEVEL = 5;  // 종족 강화
    public const int JOB_LEVEL = 10;
    public const int PASSIVE_LEVEL = 15;
    public const int ACTIVE_SKILL_LEVEL = 20; // 아직 없음

    // 레벨업 창 한 번의 카드 구성에 필요한 값
    public class Input
    {
        public int level;
        public bool hasRace, hasJob, hasTileExpand, hasPassive;
        public IList<AbilityData> racePool, jobPool, passivePool, statPool;
        public AbilityData tileExpand;
        public AbilityData ownedPassive; // 고른 패시브 (최대 레벨 전까지 능력치 카드에 강화 카드로 섞임)
        public Func<AbilityData, int> getLevel;
    }

    // 이번 레벨업 창의 단계 (Stat = 능력치 카드)
    public static AbilityCategory GetStage(Input input)
    {
        if (!input.hasRace)
            return AbilityCategory.Race;
        if (!input.hasTileExpand && input.level >= TILE_EXPAND_LEVEL && input.tileExpand != null)
            return AbilityCategory.TileExpand;
        if (input.hasTileExpand && !input.hasJob && input.level >= JOB_LEVEL && HasAny(input.jobPool))
            return AbilityCategory.Job;
        if (input.hasJob && !input.hasPassive && input.level >= PASSIVE_LEVEL && HasAny(input.passivePool))
            return AbilityCategory.Passive;
        return AbilityCategory.Stat;
    }

    static bool HasAny(IList<AbilityData> pool) => pool != null && pool.Count > 0;

    // 레벨업 버튼 글씨 (단계 이름, 번역 전 임시)
    public static string GetStageName(AbilityCategory stage)
    {
        switch (stage)
        {
            case AbilityCategory.Race: return "종족 선택";
            case AbilityCategory.TileExpand: return "종족 강화";
            case AbilityCategory.Job: return "직업 선택";
            case AbilityCategory.Passive: return "패시브 선택";
            case AbilityCategory.ActiveSkill: return "스킬 선택";
            default: return "레벨 업";
        }
    }

    public static List<AbilityData> GetChoices(Input input, System.Random rng, int count = CHOICE_COUNT)
    {
        Func<AbilityData, int> getLevel = input.getLevel ?? (_ => 0);
        switch (GetStage(input))
        {
            case AbilityCategory.Race:
            {
                List<AbilityData> races = PickStatChoices(input.racePool, getLevel, rng, count);
                if (races.Count > 0)
                    return races;
                break;
            }
            case AbilityCategory.TileExpand:
            {
                var expands = new List<AbilityData>();
                for (int i = 0; i < count; i++)
                    expands.Add(input.tileExpand);
                return expands;
            }
            case AbilityCategory.Job:
                return PickStatChoices(input.jobPool, getLevel, rng, count);
            case AbilityCategory.Passive:
                return PickStatChoices(input.passivePool, getLevel, rng, count);
        }

        var pool = new List<AbilityData>();
        if (input.statPool != null)
            pool.AddRange(input.statPool);
        if (input.ownedPassive != null)
            pool.Add(input.ownedPassive); // 최대 레벨이면 PickStatChoices가 뺌
        return PickStatChoices(pool, getLevel, rng, count);
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

using System;
using System.Collections.Generic;

// (2026-11-20) 레벨업 카드 구성 규칙 (씬 없이 계산 → 테스트 가능)
// - 종족이 아직 없으면 = 종족 카드 (Lv.1 첫 레벨업. 한 번에 여러 레벨이 올라도 첫 창은 종족), 5종 중 랜덤 3개
// - 그 외 = 킹 능력치 카드 (최대 레벨인 카드 제외, 서로 다른 카드 count장)
// - 이후 단계: Lv.5 직업 / Lv.10 패시브 / Lv.15 발동 스킬이 여기 들어올 예정
public static class LevelUpChoiceRule
{
    public const int CHOICE_COUNT = 3;

    public static List<AbilityData> GetChoices(int level, bool hasRace, IList<AbilityData> racePool, IList<AbilityData> statPool,
        Func<AbilityData, int> getLevel, Random rng, int count = CHOICE_COUNT)
    {
        if (!hasRace)
        {
            List<AbilityData> races = PickStatChoices(racePool, getLevel, rng, count);
            if (races.Count > 0)
                return races;
        }
        return PickStatChoices(statPool, getLevel, rng, count);
    }

    // pool에서 최대 레벨이 아닌 서로 다른 카드를 랜덤으로 count장
    public static List<AbilityData> PickStatChoices(IList<AbilityData> pool, Func<AbilityData, int> getLevel, Random rng, int count = CHOICE_COUNT)
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

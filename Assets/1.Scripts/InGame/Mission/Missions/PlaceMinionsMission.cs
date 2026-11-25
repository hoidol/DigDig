using System;
using System.Collections.Generic;
using UnityEngine;

// 필드에 레벨 N 미니언 M기가 동시에 있으면 완료 (예: 서로 다른 미니언 Lv.2 5기)
// 레벨 = 화면 표시 레벨(mergeLevel + 1). 판정 = 지금 필드 상황 (동시에 있어야 함)
// (2026-11-25) exactLevel 켜면 정확히 그 레벨만 셈 (Lv.1 미션에 Lv.2 이상은 안 셈)
[Serializable]
public class PlaceMinionsMission : Mission
{
    [Tooltip("표시 레벨 (1부터)")]
    public int minLevel = 1;
    [Tooltip("켜면 정확히 그 레벨만 셈, 끄면 그 레벨 이상")]
    public bool exactLevel = true;
    [Tooltip("필요한 미니언 수")]
    public int count = 5;
    [Tooltip("켜면 서로 다른 종만 셈 (같은 종 여러 기는 1기)")]
    public bool distinctKeys = true;

    public override string TitleKey => distinctKeys ? "Mission_PlaceDistinctMinions" : "Mission_PlaceMinions";
    protected override object[] GetTitleArgs() => new object[] { minLevel, count };

    public int CountMatching(MissionContext ctx)
    {
        int matched = 0;
        HashSet<string> keys = distinctKeys ? new HashSet<string>() : null;
        foreach (var m in ctx.fieldMinions)
        {
            int level = m.mergeLevel + 1;
            if (exactLevel ? level != minLevel : level < minLevel)
                continue;
            if (keys != null && !keys.Add(m.key))
                continue;
            matched++;
        }
        return matched;
    }

    public override bool IsComplete(MissionContext ctx) => CountMatching(ctx) >= count;
    public override (int cur, int goal) GetProgress(MissionContext ctx) => (Math.Min(CountMatching(ctx), count), count);
}

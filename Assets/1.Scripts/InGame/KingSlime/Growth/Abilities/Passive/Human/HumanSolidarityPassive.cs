using System.Collections.Generic;
using UnityEngine;

// 인간 · 버프형 · 연대: 버프 칸 미니언이 2마리 이상이고 모두 다른 종이면 모든 칸 버프 ×scale
// 의도: 오크(부족의 결속 = 같은 종)와 반대로 다양한 종을 킹 주변에 섞게 함
public class HumanSolidarityPassive : PassiveKingAbility, IKingBuffAreaScaler
{
    public float[] scales = { 1.5f, 1.75f, 2f };
    public int minCount = 2;

    public override object[] GetDescArgs(int level) => new object[] { FormatNumber(LV(scales, level)) };

    protected override void Apply() { Area.RemoveScaler(this); Area.AddScaler(this); }
    protected override void Clear() => Area.RemoveScaler(this);

    public float GetBuffScale(IReadOnlyList<Minion> targets)
    {
        if (targets.Count < minCount)
            return 1f;
        var keys = new HashSet<string>();
        foreach (Minion m in targets)
            if (!keys.Add(m.key))
                return 1f;
        return LV(scales);
    }
}

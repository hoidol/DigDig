using System.Collections.Generic;
using UnityEngine;

// 오크 · 버프형 · 부족의 결속: 버프 칸 미니언이 2마리 이상이고 모두 같은 종이면 모든 칸 버프 ×scale
// 의도: 인간(연대 = 모두 다른 종)과 반대로 한 종을 킹 주변에 몰게 함
public class OrcTribalBondPassive : PassiveKingAbility, IKingBuffAreaScaler
{
    public float[] scales = { 2f, 2.5f, 3f };
    public int minCount = 2;

    public override object[] GetDescArgs(int level) => new object[] { FormatNumber(LV(scales, level)) };

    protected override void Apply() { Area.RemoveScaler(this); Area.AddScaler(this); }
    protected override void Clear() => Area.RemoveScaler(this);

    public float GetBuffScale(IReadOnlyList<Minion> targets)
    {
        if (targets.Count < minCount)
            return 1f;
        string key = targets[0].key;
        foreach (Minion m in targets)
            if (m.key != key)
                return 1f;
        return LV(scales);
    }
}

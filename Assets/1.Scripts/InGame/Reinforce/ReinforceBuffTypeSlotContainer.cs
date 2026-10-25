using System;
using System.Linq;
using UnityEngine;

// 강화 종류 릴 (자세한 구성은 기획 확정 후. 일단 ReinforceBuffType 전체)
public class ReinforceBuffTypeSlotContainer : ReinforceSlotContainer
{
    public Sprite[] buffThums; // ReinforceBuffType 순서대로 (None 포함)

    protected override string[] GetKeys()
    {
        return Enum.GetNames(typeof(ReinforceBuffType)).ToArray();
    }

    protected override Sprite GetThum(string key)
    {
        int idx = (int)Enum.Parse<ReinforceBuffType>(key);
        return buffThums != null && idx < buffThums.Length ? buffThums[idx] : null;
    }
}

using System;
using UnityEngine;

// 강화 수치 릴 (자세한 구성은 기획 확정 후. 일단 인스펙터에 넣은 values 사용)
public class ReinforceValueSlotContainer : ReinforceSlotContainer
{
    public string[] values;
    public Sprite[] valueThums; // values와 같은 순서

    protected override string[] GetKeys()
    {
        values = new string[4];
        
        values[0] = "20%";
        values[1] = "25%";
        values[2] = "30%";
        values[3] = "35%";
        return values;
    }

    protected override Sprite GetThum(string key)
    {
        int idx = Array.IndexOf(values, key);
        return valueThums != null && idx >= 0 && idx < valueThums.Length ? valueThums[idx] : null;
    }
}

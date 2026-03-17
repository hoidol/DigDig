using UnityEngine;

[CreateAssetMenu(fileName = "StatUpAbilityData", menuName = "Ability/StatUpAbilityData", order = 0)]
public class StatUpAbilityData : AbilityData
{
    public StatType statType;
    public float initValue;
    public float increasedValue;

    public float GetValue(int count)
    {
        return initValue + increasedValue * count;
    }

    public StatOpType opType;   // 어떤 방식으로 적용할지

    public float Apply(float baseValue, int count)
    {
        float v = GetValue(count);
        switch (opType)
        {
            case StatOpType.Add:
                return baseValue + v;
            case StatOpType.Subtract:
                return baseValue - v;
            case StatOpType.Multiply:
                return baseValue * v;
            case StatOpType.Divide:
                return v != 0 ? baseValue / v : baseValue;
            default:
                return baseValue;
        }
    }
}

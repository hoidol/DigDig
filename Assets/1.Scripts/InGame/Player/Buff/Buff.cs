[System.Serializable]
public class Buff
{
    public StatType statType;
    public float value;
    public StatOpType opType;

    public Buff(StatType statType, float value, StatOpType opType)
    {
        this.statType = statType;
        this.value = value;
        this.opType = opType;
    }

    public float Apply(float baseValue)
    {
        switch (opType)
        {
            case StatOpType.Add: return baseValue + value;
            case StatOpType.Subtract: return baseValue - value;
            case StatOpType.Multiply: return baseValue * value;
            case StatOpType.Divide: return value != 0 ? baseValue / value : baseValue;
            default: return baseValue;
        }
    }
}

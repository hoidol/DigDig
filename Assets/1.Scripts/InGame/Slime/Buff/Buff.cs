[System.Serializable]
public class Buff
{
    public StatType statType;
    public float value;
    public StatOpType opType;
    // 받는 동안 미니언을 따라다니는 이펙트 (null = 없음). 같은 종류는 버프가 여러 개여도 하나만 (Minion.RefreshBuffEffects)
    public EffectType? effectType;

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
            case StatOpType.Multiply: return baseValue * value;
            default: return baseValue;
        }
    }
}

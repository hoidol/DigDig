public class StatData
{
    public static string GetValueToString(StatType statType, float value)
    {
        switch (statType)
        {
            case StatType.AttackPower:
            case StatType.MaxHp:
                return $"{value}";
            case StatType.AttackSpeed:
                return $"10초당 {value}발";
            case StatType.RecoveryHp:
                return $"초당 +{value}";
            case StatType.MoveSpeed:
                return $"초당 {value}m";
            case StatType.CritChance:
                return $"{value*100:0.0}%";
            case StatType.CritPower:
                return $"{value*100:0.0}%";
            case StatType.Dodge:
                return $"{value*100:0.0}%";
        }
        return $"{value}";
    } 

}
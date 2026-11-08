using UnityEngine;

public class StatData
{
    public const string ICON_PATH = "Icons/Stat"; // Resources/Icons/Stat/{StatType}

    // StatType과 이름이 같은 스탯 아이콘 (없으면 null)
    public static Sprite GetThum(StatType statType)
    {
        return Resources.Load<Sprite>($"{ICON_PATH}/{statType}");
    }

    // 스탯 이름. 번역 key = {StatType}_title
    public static string GetTitle(StatType statType)
    {
        return TranslateManager.GetText($"{statType}_title");
    }

    public static string GetValueToString(StatType statType, float value)
    {
        switch (statType)
        {
            case StatType.AttackPower:
            case StatType.MaxHp:
                return $"{value}";
            case StatType.AttackSpeed:
                // AttackSpeed가 ATTACK_SPEED_PER_SECOND면 초당 1발 (Weapon 공격 타이머와 같은 기준)
                return string.Format(TranslateManager.GetText("Stat_AttackSpeedValue"), value * 10f / GameSetting.ATTACK_SPEED_PER_SECOND);
            // case StatType.RecoveryHp:
            //     return $"초당 +{value}";
            // case StatType.MoveSpeed:
            //     return $"초당 {value}m";
            case StatType.CritChance:
                return $"{value:0.0}%"; // 인게임 치명타 확률은 % 단위 (AllyUnitDamageData.critChance)
            case StatType.CritPower:
                return $"{value*100:0.0}%";
            // case StatType.Dodge:
            //     return $"{value*100:0.0}%";
        }
        return $"{value}";
    } 

}
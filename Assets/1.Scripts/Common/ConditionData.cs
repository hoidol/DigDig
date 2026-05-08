[System.Serializable]
public class ConditionData
{
    public ConditionType conditionType;
    public string value;
    public int count;

    public bool Check()
    {
        if (conditionType == ConditionType.NeedAbility)
        {
            Ability ability = Player.Instance.abilityInventory.GetAbility(value);
            if (ability == null || ability.count <= 0)
                return false;

        }
        else if (conditionType == ConditionType.TotalAbilityCount)
        {
            if (Player.Instance.abilityInventory.abilityCount < count)
                return false;
        }
        else if (conditionType == ConditionType.NeedAbilityLevel)
        {
            Ability ability = Player.Instance.abilityInventory.GetAbility(value);
            if (ability == null || ability.count < count)
                return false;
        }
        return true;
    }
}

public enum ConditionType
{
    NeedAbility,        // 해당 어빌리티 보유 여부
    TotalAbilityCount,  // 어빌리티 총 개수
    NeedAbilityLevel,   // 해당 어빌리티가 count 레벨 이상 (value = 어빌리티 key, count = 필요 레벨)
    NeedItem,
    NeedItemCount,
    Count,
}
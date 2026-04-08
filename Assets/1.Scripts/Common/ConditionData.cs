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
        return true;
    }
}

public enum ConditionType
{

    NeedAbility, //필요한 스텟
    TotalAbilityCount, //필요한 스탯량
    Count,
}
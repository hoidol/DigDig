[System.Serializable]
public class ConditionData
{
    public ConditionType conditionType;
    public string value;
    public int count;

    public bool Check()
    {
        return true;
    }
}

public enum ConditionType
{

    Count,
}
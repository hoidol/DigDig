using UnityEngine;

[CreateAssetMenu(fileName = "AbilityData", menuName = "Ability/AbilityData", order = 0)]
public class AbilityData : ScriptableObject
{
    public string key;
    public Sprite thum;
    public Grade grade;
    public int maxLv;
    public ConditionData[] conditions;

    public bool Unlocked()
    {
        foreach (var condition in conditions)
        {
            if (!condition.Check())
                return false;
        }
        return true;
    }
    public string Title => key;
    public string Description()
    {
        return "설명";
    }
#if UNITY_EDITOR
    public void Edit()
    {
        Debug.Log("AbilityData Edit()");
        if (string.IsNullOrEmpty(key) || key != name)
        {
            key = name;
        }

    }
#endif


}

public enum Grade
{
    Normal,
    Rare,
    Unique,
    Legend,
    Myth
}
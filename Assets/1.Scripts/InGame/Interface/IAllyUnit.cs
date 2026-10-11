using UnityEngine;

public interface IAllyUnit
{
    float AccumulatedDamage { get; set; }
    void AccumulateDamage(float d);

    public void SetFindTargetType(FindTargetType type);
    public FindTargetType FindTargetType
    {
        get;
    }

    public void StartTarget();

    public void EndTarget();
    Transform Transform
    {
        get;
    }
    string Key
    {
        get;
    }
    AllyType AllyType
    {
        get;
    }
}

public enum AllyType
{
    Character,
    Slime
}
using UnityEngine;

public interface IAllyUnit
{
    float AccumulatedDamage{get;set;}
    void AccumulateDamage(float d);
    
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
    MiniMe
}
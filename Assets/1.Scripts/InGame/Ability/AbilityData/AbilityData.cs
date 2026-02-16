using UnityEngine;

//[CreateAssetMenu(fileName = "AbilityData", menuName = "AbilityData", order = 0)]
public class AbilityData : ScriptableObject
{
    public string key;
    public Sprite thum;

}

public enum Grade
{
    Normal,
    Rare,
    Unique,
    Legend,
    Myth
}
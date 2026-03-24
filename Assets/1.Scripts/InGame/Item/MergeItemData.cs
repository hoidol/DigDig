using UnityEngine;

[CreateAssetMenu]
public class MergeItemData : ScriptableObject
{
    public string item1Key;
    public string item2Key; //무조건 3등급부터 머지할 수 있음 
}
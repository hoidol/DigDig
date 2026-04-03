using UnityEngine;

[CreateAssetMenu]
public class MergeItemData : ScriptableObject
{
    //무조건 3등급부터 머지할 수 있음 
    public string[] resourceItemKeys;
    public string resultItemKey;
}
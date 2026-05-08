using UnityEngine;

[CreateAssetMenu]
public class MergeItemData : ScriptableObject
{
    public string[] resourceItemKeys;
    public string[] resourceAbilityKeys;
    public bool isHidden;
    public string resultItemKey;
    public static MergeItemData GetMergeItemData(string resultKey)
    {
        return ItemManager.Instance.GetMergeItemData(resultKey);
    }
}

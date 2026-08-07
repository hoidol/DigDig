using UnityEngine;

[CreateAssetMenu]
public class MergeItemData : ScriptableObject
{
    public string[] childItemKeys;
    public string resultItemKey;
    public static MergeItemData GetMergeItemData(string resultKey)
    {
        return ItemManager.Instance.GetMergeItemData(resultKey);
    }
}

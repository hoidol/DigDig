using UnityEngine;

[CreateAssetMenu]
public class MergeRuneData : ScriptableObject
{
    public string[] childRuneKeys;
    public string resultRuneKey;
    public static MergeRuneData GetMergeRuneData(string resultKey)
    {
        return RuneManager.Instance.GetMergeRuneData(resultKey);
    }
}

using UnityEngine;

[CreateAssetMenu]
public class MergeBulletData : ScriptableObject
{
    public string[] resourceBulletKeys;
    public string resultBulletKey;
    public static MergeBulletData GetMergeBulletData(string resultKey)
    {
        return BulletManager.Instance.GetMergeBulletData(resultKey);
    }
}
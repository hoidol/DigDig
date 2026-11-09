using UnityEngine;

public class KingSlimeManager : MonoSingleton<KingSlimeManager>
{
    public KingSlimeData[] kingslimeDatas;
    private void Awake()
    {
        kingslimeDatas = Resources.LoadAll<KingSlimeData>($"KingSlimeData");
    }

    public KingSlimeData GetKingSlimeData(KingSlimeName kingslimeName)
    {
        Debug.Log($"CharacterManager GetCharacterData Start {kingslimeName} 찾아");
        for (int i = 0; i < kingslimeDatas.Length; i++)
        {
            if (kingslimeDatas[i].kingslimeName == kingslimeName)
            {
                Debug.Log($"CharacterManager GetCharacterData {kingslimeName}");
                return kingslimeDatas[i];
            }
        }
        return null;
    }

    // 기본값 + 로비 업그레이드 (권능은 스탯 없음)
    public float GetTotalStatValue(KingSlimeData kingslimeData, StatType statType)
    {
        float baseValue = kingslimeData.GetKingSlimeStat(statType)?.value ?? 0f;
        return baseValue + UserDataManager.Instance.userKingSlimeManager.GetUpgradeValue(statType);
    }
}
public enum KingSlimeName
{
    Lucky
}
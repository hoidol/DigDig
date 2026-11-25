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

    // 기본값 + 유저 레벨 스탯 포인트 (2026-11-24, 이전: 로비 재화 업그레이드)
    public float GetTotalStatValue(KingSlimeData kingslimeData, StatType statType)
    {
        float baseValue = kingslimeData.GetKingSlimeStat(statType)?.value ?? 0f;
        return GetTotalStatValue(baseValue, statType);
    }

    // 기본값(KingSlimeData 또는 용사 HeroData) + 용사 강화 포인트
    public float GetTotalStatValue(float baseValue, StatType statType) =>
        UserDataManager.Instance.userKingSlimeManager.ApplyStatPoints(statType, baseValue);
}
public enum KingSlimeName
{
    Lucky
}
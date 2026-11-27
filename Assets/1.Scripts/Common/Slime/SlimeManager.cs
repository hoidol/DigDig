using UnityEngine;

public class SlimeManager : MonoSingleton<SlimeManager>
{
    public SlimeData[] slimeDatas;
    private void Awake()
    {
        slimeDatas = Resources.LoadAll<SlimeData>($"SlimeData");
    }

    public SlimeData GetSlimeData(SlimeName slimeName)
    {
        Debug.Log($"CharacterManager GetCharacterData Start {slimeName} 찾아");
        for (int i = 0; i < slimeDatas.Length; i++)
        {
            if (slimeDatas[i].slimeName == slimeName)
            {
                Debug.Log($"CharacterManager GetCharacterData {slimeName}");
                return slimeDatas[i];
            }
        }
        return null;
    }

    // 기본값 + 유저 레벨 스탯 포인트 (2026-11-24, 이전: 로비 재화 업그레이드)
    public float GetTotalStatValue(SlimeData slimeData, StatType statType)
    {
        float baseValue = slimeData.GetSlimeStat(statType)?.value ?? 0f;
        return GetTotalStatValue(baseValue, statType);
    }

    // 기본값(SlimeData 또는 용사 HeroData) + 용사 강화 포인트
    public float GetTotalStatValue(float baseValue, StatType statType) =>
        UserDataManager.Instance.userSlimeManager.ApplyStatPoints(statType, baseValue);
}
public enum SlimeName
{
    Lucky
}
using UnityEngine;

public class CharacterManager : MonoSingleton<CharacterManager>
{
    public CharacterData[] characterDatas;
    private void Awake()
    {
        characterDatas = Resources.LoadAll<CharacterData>($"CharacterData");
    }

    public CharacterData GetCharacterData(CharacterName characterName)
    {
        Debug.Log($"CharacterManager GetCharacterData Start {characterName} 찾아");
        for (int i = 0; i < characterDatas.Length; i++)
        {
            if (characterDatas[i].characterName == characterName)
            {
                Debug.Log($"CharacterManager GetCharacterData {characterName}");
                return characterDatas[i];
            }
        }
        return null;
    }

    public float GetTotalStatValue(CharacterData characterData, StatType statType, bool includeEquipment = true)
    {
        float baseValue = characterData.GetCharacterStat(statType)?.value ?? 0f;
        return baseValue + (includeEquipment ? EquipmentManager.Instance.GetSumEquipmentAbility(statType).value : 0f);
    }
}
public enum CharacterName
{
    Lucky
}
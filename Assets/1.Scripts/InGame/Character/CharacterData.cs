using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "CharacterData", order = 0)]
public class CharacterData : ScriptableObject
{
    // public string key;
    public CharacterName characterName;

    public CharacterStat[] characterStats = new CharacterStat[(int)StatType.Count];
    public string defaultBullets;
    public CharacterStat GetCharacterStat(StatType type)
    {
        for (int i = 0; i < characterStats.Length; i++)
        {
            if (characterStats[i].statType == type)
            {
                return characterStats[i];
            }
        }
        return null;
    }


#if UNITY_EDITOR
    public void Edit()
    {
        for (int i = 0; i < characterStats.Length; i++)
        {
            characterStats[i].statType = (StatType)i;
        }
    }
#endif
}
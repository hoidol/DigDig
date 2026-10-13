using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "CharacterData", order = 0)]
public class CharacterData : ScriptableObject
{
    // public string key;
    public CharacterName characterName;

    public CharacterStat[] characterStats = new CharacterStat[(int)StatType.Count];
    public string defaultBullets;
    public Sprite thum;
    public string Title => TranslateManager.GetText(characterName.ToString());
    public string Desc => TranslateManager.GetText($"{characterName}_desc");
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
    // Json/CharacterData.csv 에서 key == characterName 인 행을 읽는다.
    // 스탯 컬럼명은 StatType 이름과 같다. (StatType에 스탯을 추가하면 같은 이름의 컬럼만 추가하면 됨)
    // 컬럼이 없거나 비어 있는 스탯은 기존 값을 유지한다.
    public void LoadData()
    {
        TableReader table = TableReader.Load("CharacterData.csv");
        if (table == null) return;

        TableReader.Row row = table.Find("Key", characterName.ToString());
        if (row == null)
        {
            Debug.LogWarning($"[CharacterData] CSV에서 '{characterName}' 를 찾지 못함");
            return;
        }

        var stats = new CharacterStat[(int)StatType.Count];
        for (int i = 0; i < stats.Length; i++)
        {
            StatType statType = (StatType)i;
            stats[i] = new CharacterStat { statType = statType, value = GetCharacterStat(statType)?.value ?? 0 };

            if (row.TryGetFloat(statType.ToString(), out float value))
                stats[i].value = value;
            else if (!table.HasColumn(statType.ToString()))
                Debug.LogWarning($"[CharacterData] {characterName} '{statType}' 컬럼 없음 (기존 값 유지)");
        }
        characterStats = stats;

        // 오타 등으로 StatType과 매칭되지 않는 컬럼은 무시되므로 경고
        foreach (string header in table.headers)
        {
            if (string.IsNullOrEmpty(header)
                || string.Equals(header, "Key", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(header, "DefaultBullets", System.StringComparison.OrdinalIgnoreCase)) continue;
            if (!System.Enum.TryParse(header, true, out StatType _))
                Debug.LogWarning($"[CharacterData] '{header}' 컬럼은 StatType에 없어서 무시됨");
        }

        if (table.HasColumn("DefaultBullets"))
            defaultBullets = row.Get("DefaultBullets");

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[CharacterData] {characterName} LoadData 완료");
    }

    public void Edit()
    {
        for (int i = 0; i < characterStats.Length; i++)
        {
            characterStats[i].statType = (StatType)i;
        }
    }
#endif
}
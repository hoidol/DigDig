using UnityEngine;
[System.Serializable]
public class FullHealLevelUpStatData : LevelUpStatData
{

    public FullHealLevelUpStatData()
    {
        type = LevelUpStatType.FullHeal;
    }

    public override string GetDescription()
    {
        return TranslateManager.GetText($"{type}_description");
    }
}
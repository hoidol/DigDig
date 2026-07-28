using UnityEngine;
[System.Serializable]
public class FullHealLevelUStatData :LevelUpStatData
{
    
    public FullHealLevelUStatData()
    {
        levelUpStatType= LevelUpStatType.FullHeal;
    }

    public override string GetDescription()
    {
        return TranslateManager.GetText($"{levelUpStatType}_description");
    }
}
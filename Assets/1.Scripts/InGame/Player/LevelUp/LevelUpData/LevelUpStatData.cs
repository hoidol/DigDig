
[System.Serializable]
public abstract class LevelUpStatData 
{
    public LevelUpStatType levelUpStatType;   

    public string Title => TranslateManager.GetText($"{levelUpStatType}_title");
    public abstract string GetDescription();
}
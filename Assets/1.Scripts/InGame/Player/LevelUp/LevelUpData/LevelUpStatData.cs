
[System.Serializable]
public abstract class LevelUpStatData
{
    public LevelUpStatType type;

    public string Title => TranslateManager.GetText($"{type}_title");
    public abstract string GetDescription();
}
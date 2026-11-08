using TMPro;
using UnityEngine;

public class DifficultyDisplayButton : ButtonUI
{
    public TMP_Text difficultText;

    // LobbyManager.Awake의 GameEventBus.Clear 이후에 구독되도록 Start에서
    void Start()
    {
        GameEventBus.Subscribe<ChangedDifficultyEvent>(OnChangedDifficultyEvent);
        UpdateButton();
    }
    void OnDestroy()
    {
        GameEventBus.Unsubscribe<ChangedDifficultyEvent>(OnChangedDifficultyEvent);
    }
    void OnChangedDifficultyEvent(ChangedDifficultyEvent e)
    {
        UpdateButton();
    }

    void UpdateButton()
    {
        if (difficultText != null)
            difficultText.text = GetDifficultyText(UserDataManager.difficultyType);
    }

    // 번역 key = Difficulty.{난이도} (Normal → 보통, Hard → 어려움, Hell → 지옥)
    public static string GetDifficultyText(DifficultyType difficultyType)
    {
        return TranslateManager.GetText($"Difficulty.{difficultyType}");
    }

    public override void OnClickedBtn()
    {
        SelectDifficultyCanvas.Instance.OpenCanvas(() =>
        {
            UpdateButton();
        });
    }
}

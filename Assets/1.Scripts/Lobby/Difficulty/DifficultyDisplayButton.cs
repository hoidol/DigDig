using TMPro;
using UnityEngine;

public class DifficultyDisplayButton : ButtonUI
{
    public TMP_Text difficultText;
    void Start()
    {
        // GameEventBus.Subscribe<ChangedDifficultyEvent>(OnChangedDifficultyEvent);
    }
    void UpdateButton()
    {
        
    }
    // void OnChangedDifficultyEvent(ChangedDifficultyEvent e)
    // {
    //     UpdateButton();
    // }
    public override void OnClickedBtn()
    {
        SelectDifficultyCanvas.Instance.OpenCanvas(() =>
        {
            UpdateButton();

        });
    }
}

using System;
using TMPro;
using UnityEngine;

public class SelectDifficultyCanvas : CanvasUI<SelectDifficultyCanvas>
{

    // public override void OpenCanvas(Action closeCallback = null)
    // {
    //     base.OpenCanvas(closeCallback);

    // }

    // public void UpdateCanvas()
    // {
        
    // }

    public void SelectedDifficulty(DifficultyType difficultyType)
    {
        UserDataManager.difficultyType= difficultyType;
        GameEventBus.Publish(new ChangedDifficultyEvent(difficultyType));
        CloseCanvas();
        ToastCanvas.Toast(string.Format(TranslateManager.GetText("Difficulty_Changed"), DifficultyDisplayButton.GetDifficultyText(difficultyType)));
    }
}
public class ChangedDifficultyEvent
{
    DifficultyType difficultyType;
    public ChangedDifficultyEvent(DifficultyType difficultyType)
    {
        this.difficultyType = difficultyType;
    }
}
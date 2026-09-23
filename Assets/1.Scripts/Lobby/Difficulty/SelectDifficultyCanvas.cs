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
        ToastCanvas.Toast($"난이도 {difficultyType}가 설정되었습니다.");
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
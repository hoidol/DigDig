using TMPro;
using UnityEngine;

public class SelectDifficultyButton : ButtonUI
{
    public DifficultyType difficultyType;

    // public TMP_Text titleText;
    // public TMP_Text descText;

    public override void OnClickedBtn()
    {
        SelectDifficultyCanvas.Instance.SelectedDifficulty(difficultyType);
    }
}

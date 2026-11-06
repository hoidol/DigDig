using UnityEngine;

// RankingCanvas 난이도 탭
public class RankingDifficultyTabButton : ButtonUI
{
    public DifficultyType difficulty;
    public GameObject selectedObject; // 선택 표시 (선택)

    public override void OnClickedBtn()
    {
        RankingCanvas.Instance.SelectDifficulty(difficulty);
    }

    public void SetSelected(bool selected)
    {
        if (selectedObject != null)
            selectedObject.SetActive(selected);
    }
}

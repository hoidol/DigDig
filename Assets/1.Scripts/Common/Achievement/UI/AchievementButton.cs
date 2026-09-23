using UnityEngine;

public class AchievementButton : ButtonUI
{
    public GameObject redDot;

    public void UpdateButton()
    {
        int dC = AchievementManager.Instance.dailyAchievementManager.GetCanClearCount();
        int aC = AchievementManager.Instance.cumulativeAchievementManager.GetCanClearCount();
        redDot.SetActive(false);
        if(dC + aC > 0)
        {
            redDot.SetActive(true);
        }

    }

    public override void OnClickedBtn()
    {
        AchievementCanvas.Instance.OpenCanvas(() =>
        {
            UpdateButton();
        });
    }
}

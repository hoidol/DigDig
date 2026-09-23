using UnityEngine;

public class AchievementCategoryTapButton : TapButton
{
    public AchievementCategory category;
    public GameObject redDot;
    public override void UpdateButton()
    {
        base.UpdateButton();
        int count = AchievementManager.Instance.GetSubAchievementManager(category).GetCanClearCount();
        redDot.SetActive(count >0);
    }

}

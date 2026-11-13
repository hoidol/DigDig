using System;
using UnityEngine;

public class AchievementCanvas : CanvasUI<AchievementCanvas>
{
    public AchievementContainer[] achievementContainers;
    public AchievementCategoryTapButton[] categoryTapButtons;

    void OnEnable()
    {
        GameEventBus.Subscribe<UpdateAchievementEvent>(OnUpdateAchievementEvent);
    }
    void OnDisable()
    {
        GameEventBus.Unsubscribe<UpdateAchievementEvent>(OnUpdateAchievementEvent);
    }

    void OnUpdateAchievementEvent(UpdateAchievementEvent e)
    {
        UpdateCanvas();
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        OpenCanvas();
    }

    public void OpenCanvas()
    {
        AchievementManager.Instance.CheckNewDay(); // 날짜가 바뀌었으면 일일 업적 초기화
        for (int i = 0; i < achievementContainers.Length; i++)
        {
            achievementContainers[i].OpenContainer();
        }
        UpdateCanvas();
    }

    public void UpdateCanvas()
    {
        CurrencyPanel.UpdateAllPanels(); // 보상으로 받은 재화 표시
        for (int i = 0; i < achievementContainers.Length; i++)
        {
            achievementContainers[i].UpdateContainer();
        }
        for (int i = 0; i < categoryTapButtons.Length; i++)
        {
            categoryTapButtons[i].UpdateButton();
        }
    }


}
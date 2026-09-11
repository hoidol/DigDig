using System;
using UnityEngine;

public class AchievementCanvas : CanvasUI<AchievementCanvas>
{
    public AchievementContainer[] achievementContainers;

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        OpenCanvas();
    }

    public void OpenCanvas()
    {
        for(int i = 0; i < achievementContainers.Length; i++)
        {
            achievementContainers[i].OpenContainer();
        }
        UpdateCanvas();
    }

    public void UpdateCanvas()
    {
        for(int i = 0; i < achievementContainers.Length; i++)
        {
            achievementContainers[i].UpdateContainer();
        }
    }


}
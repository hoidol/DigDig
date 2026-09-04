using System;
using UnityEngine;

public class EnhanceStoneCanvas : CanvasUI<EnhanceStoneCanvas> 
{
    // 1. Growth1 슬라임 2마리 다음 레벨로 레벨업
    // 2. Growth1 - level2 끼리 조합해서 Growth2 만들기
    // 3. 아이템 뽑기
    public LevelUpSlimeButton levelUpSlimeButton;
    public MergeSlimeButton mergeSlimeButton;
    public DrawItemButton drawItemButton;
    public void OnEnable()
    {
        Time.timeScale = 0;
    }
    public void OnDisable()
    {
        Time.timeScale = 1;
    }
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        OpenCanvas();
    }
    public void OpenCanvas()
    {
        UpdateCanvas();
    }
    public void UpdateCanvas()
    {
        mergeSlimeButton.UpdateButton();
        levelUpSlimeButton.UpdateButton();
        drawItemButton.UpdateButton();
    }

}
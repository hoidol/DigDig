using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StatCanvas : CanvasUI<StatCanvas>
{
    public StatPanel[] statPanels; //마지막은 기억의 파편에서 오픈 가능

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        Time.timeScale = 0f;
        if (statPanels.Length <= 0)
            statPanels = GetComponentsInChildren<StatPanel>();

        //선택 가능한 능력치 걸러고 뽑기 진행

        UpdateCanvas();
    }
    void UpdateCanvas()
    {
        List<StatData> statDatas = StatManager.Instance.GetStatDatas(statPanels.Length);
        for (int i = 0; i < statPanels.Length; i++)
        {
            //75% 20% 5%로 레벨 1,2,3 뽑기
            int lv = 1;
            if (UnityEngine.Random.Range(0f, 100) < 5)
                lv = 3;
            else if (UnityEngine.Random.Range(0f, 100) < 20)
                lv = 2;
            if (i < statDatas.Count)
                statPanels[i].SetStatData(statDatas[i], lv);
            else
                statPanels[i].SetStatData(null, 0);
        }

    }
    public void OnClickedReset()
    {
        UpdateCanvas();
    }


    public override void CloseCanvas()
    {
        base.CloseCanvas();
        Time.timeScale = 1f;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCanvas : CanvasUI<AbilityCanvas>
{
    public AbilityPanel[] abilityPanels; //마지막은 기억의 파편에서 오픈 가능

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        //선택 가능한 능력치 걸러고 뽑기 진행

        UpdateCanvas();
    }
    void UpdateCanvas()
    {
        List<AbilityData> abilityDatas = AbilityManager.Instance.GetAbilityDatas(4);
        for (int i = 0; i < abilityPanels.Length; i++)
        {
            if (i < abilityDatas.Count)
                abilityPanels[i].SetAbilityData(abilityDatas[i]);
            else
                abilityPanels[i].SetAbilityData(null);
        }

    }
    public void OnClickedReset()
    {
        UpdateCanvas();


    }
}

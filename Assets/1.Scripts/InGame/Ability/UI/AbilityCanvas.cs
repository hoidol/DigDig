using System;
using UnityEngine;

public class AbilityCanvas : CanvasUI<AbilityCanvas>
{
    public AbilityPanel[] abilityPanels;
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        //선택 가능한 능력치 걸러고 뽑기 진행

        AbilityData[] abilityDatas = AbilityManager.Instance.GetAbilityDatas();
        for (int i = 0; i < abilityDatas.Length; i++)
        {
            abilityPanels[i].SetAbilityData(abilityDatas[i]);
        }
    }
}

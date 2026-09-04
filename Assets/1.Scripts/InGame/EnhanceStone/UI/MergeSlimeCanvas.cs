using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MergeSlimeCanvas : CanvasUI<MergeSlimeCanvas>
{
    public MergeSlimePanel[] mergeSlimePanels;

    UniTaskCompletionSource<string> tcs;
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        Init();   
        Open();
    }
    void Open()
    {
        UpdateCanvas();
    }

    void UpdateCanvas()
    {
        string[] level2Slimes = Character.Instance.slimeInventory.curSlimes
            .Where(slime => slime.level == 2)
            .Select(slime => slime.key)
            .ToArray();
         List<SlimeMergeData>  canMakeSlimes = SlimeManager.Instance.GetSlimeMergeDatas(level2Slimes);
        for (int i = 0; i < mergeSlimePanels.Length; i++)
        {
            if (i < canMakeSlimes.Count)
                mergeSlimePanels[i].Set(canMakeSlimes[i], OnPanelClicked);
            else
                mergeSlimePanels[i].Hide();
        }
    }

    void OnPanelClicked(SlimeMergeData mergeData, string key)
    {
        tcs?.TrySetResult(key);
        
    }

    List<SlimeMergeData> GetCanMakeSlimes(Slime slime1, Slime slime2)
    {
        //총 3가지
        //1. slime1 + slime2 조합 전용 특수 2단계 미니미 (growth1SlimeKeys에 둘 다 포함)
        //2. slime1의 일반 2단계 형태 (growth1SlimeKeys가 slime1.key 하나뿐)
        //3. slime2의 일반 2단계 형태 (growth1SlimeKeys가 slime2.key 하나뿐)

        List<SlimeMergeData> canMakeSlimes = new List<SlimeMergeData>();
        SlimeMergeData[] conditionDatas = SlimeManager.Instance.slimeMergeDatas;
        for (int i = 0; i < conditionDatas.Length; i++)
        {
            string[] requireKeys = conditionDatas[i].growth1SlimeKeys;
            if (requireKeys.Length == 1)
            {
                if (requireKeys[0] == slime1.key || requireKeys[0] == slime2.key)
                    canMakeSlimes.Add(conditionDatas[i]);
            }
            else if (requireKeys.Contains(slime1.key) && requireKeys.Contains(slime2.key))
            {
                canMakeSlimes.Add(conditionDatas[i]);
            }
        }
        return canMakeSlimes;
    }
}

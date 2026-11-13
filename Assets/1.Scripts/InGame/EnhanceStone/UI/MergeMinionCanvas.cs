using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MergeMinionCanvas : CanvasUI<MergeMinionCanvas>
{
    public MergeMinionPanel[] mergeMinionPanels;

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
        string[] level2Minions = KingSlime.Instance.minionInventory.curMinions
            .Where(minion => minion.mergeLevel == MinionData.MAX_MERGE_LEVEL)
            .Select(minion => minion.key)
            .ToArray();
         List<MinionMergeData>  canMakeMinions = MinionManager.Instance.GetMinionMergeDatas(level2Minions);
        for (int i = 0; i < mergeMinionPanels.Length; i++)
        {
            if (i < canMakeMinions.Count)
                mergeMinionPanels[i].Set(canMakeMinions[i], OnPanelClicked);
            else
                mergeMinionPanels[i].Hide();
        }
    }

    void OnPanelClicked(MinionMergeData mergeData, string key)
    {
        tcs?.TrySetResult(key);
        
    }

    List<MinionMergeData> GetCanMakeMinions(Minion minion1, Minion minion2)
    {
        //총 3가지
        //1. minion1 + minion2 조합 전용 특수 2단계 미니미 (growth1MinionKeys에 둘 다 포함)
        //2. minion1의 일반 2단계 형태 (growth1MinionKeys가 minion1.key 하나뿐)
        //3. minion2의 일반 2단계 형태 (growth1MinionKeys가 minion2.key 하나뿐)

        List<MinionMergeData> canMakeMinions = new List<MinionMergeData>();
        MinionMergeData[] conditionDatas = MinionManager.Instance.minionMergeDatas;
        for (int i = 0; i < conditionDatas.Length; i++)
        {
            string[] requireKeys = conditionDatas[i].growth1MinionKeys;
            if (requireKeys.Length == 1)
            {
                if (requireKeys[0] == minion1.key || requireKeys[0] == minion2.key)
                    canMakeMinions.Add(conditionDatas[i]);
            }
            else if (requireKeys.Contains(minion1.key) && requireKeys.Contains(minion2.key))
            {
                canMakeMinions.Add(conditionDatas[i]);
            }
        }
        return canMakeMinions;
    }
}

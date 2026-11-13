using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MergeMinionButton : EnhanceStoneButton
{

    public void UpdateButton()
    {
        string[] level2Minions = KingSlime.Instance.minionInventory.curMinions
            .Where(minion => minion.mergeLevel == MinionData.MAX_MERGE_LEVEL)
            .Select(minion => minion.key)
            .ToArray();
         List<MinionMergeData>  canMakeMinions = MinionManager.Instance.GetMinionMergeDatas(level2Minions);
        titleText.text = $"융합하기 {canMakeMinions.Count}";
    }
    public override void OnClickedBtn()
    {
        MergeMinionCanvas.Instance.OpenCanvas();   
    }
}
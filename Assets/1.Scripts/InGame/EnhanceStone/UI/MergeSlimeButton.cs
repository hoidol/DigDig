using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MergeSlimeButton : EnhanceStoneButton
{

    public void UpdateButton()
    {
        string[] level2Slimes = Character.Instance.slimeInventory.curSlimes
            .Where(slime => slime.level == 2)
            .Select(slime => slime.key)
            .ToArray();
         List<SlimeMergeData>  canMakeSlimes = SlimeManager.Instance.GetSlimeMergeDatas(level2Slimes);
        titleText.text = $"융합하기 {canMakeSlimes.Count}";
    }
    public override void OnClickedBtn()
    {
        MergeSlimeCanvas.Instance.OpenCanvas();   
    }
}
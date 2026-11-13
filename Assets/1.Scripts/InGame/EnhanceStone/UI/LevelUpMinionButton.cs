using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelUpMinionButton : EnhanceStoneButton
{
    bool canLevelUp = false;
    public void UpdateButton()
    {
        int count = KingSlime.Instance.minionInventory.curMinions.Count(minion => minion.MinionData.growth == 1 && minion.mergeLevel < MinionData.MAX_MERGE_LEVEL);
        titleText.text = $"1~3마리 슬라임 레벨 업하기";

        canLevelUp = count > 1;
    }

    public override void OnClickedBtn()
    {
        if(!canLevelUp)
            return;
        List<Minion> minions = KingSlime.Instance.minionInventory.curMinions.Where(minion => minion.MinionData.growth == 1 && minion.mergeLevel < MinionData.MAX_MERGE_LEVEL).ToList();
        Minion[] selectedMinions = minions.OrderBy(s => UnityEngine.Random.value).Take(Random.Range(1,4)).ToArray();
        
        LevelUpMinionCanvas.Instance.OpenCanvas(selectedMinions, () =>
        {
        });
        for(int i =0;i<selectedMinions.Length;i++)
        {
            MinionFieldController.Instance.LevelUp(selectedMinions[i]);
        }

    }

}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelUpSlimeButton : EnhanceStoneButton
{
    bool canLevelUp = false;
    public void UpdateButton()
    {
        int count = Character.Instance.slimeInventory.curSlimes.Count(slime => slime.SlimeData.growth == 1 && slime.level < 2);
        titleText.text = $"1~3마리 슬라임 레벨 업하기";

        canLevelUp = count > 1;
    }

    public override void OnClickedBtn()
    {
        if(!canLevelUp)
            return;
        List<Slime> slimes = Character.Instance.slimeInventory.curSlimes.Where(slime => slime.SlimeData.growth == 1 && slime.level < 2).ToList();
        Slime[] selectedSlimes = slimes.OrderBy(s => UnityEngine.Random.value).Take(Random.Range(1,4)).ToArray();
        
        LevelUpSlimeCanvas.Instance.OpenCanvas(selectedSlimes, () =>
        {
        });
        for(int i =0;i<selectedSlimes.Length;i++)
        {
            SlimeSpawner.Instance.LevelUp(selectedSlimes[i]);
        }

    }

}
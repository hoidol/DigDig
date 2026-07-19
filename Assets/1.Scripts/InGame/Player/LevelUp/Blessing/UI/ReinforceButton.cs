using System.Collections.Generic;
using UnityEngine;

public class ReinforceButton : BlessingButton
{
    public override void UpdateButton()
    {
        titleText.text = TranslateManager.GetText($"Reinforce_Title");

        bool active = false;
        foreach (var bulletStat in Player.Instance.statMgr.bulletStatDic.Values)
        {
            // if (Player.Instance.weapon.bulletInventory.CheckHave(bulletStat.key) && bulletStat.lv < BulletData.MAX_LEVEL)
            // {
            //     continue;
            // }
            active = true;
            break;
        }

        for (int i = 0; i < Player.Instance.itemInventory.curItems.Count; i++)
        {
            if (Player.Instance.statMgr.GetPlayerItemStat(Player.Instance.itemInventory.curItems[i].key).lv < ItemData.MAX_LEVEL)
            {
                continue;
            }
            active = true;
            break;
        }
        gameObject.SetActive(active);
    }
    public override void OnClickedBtn()
    {
        BlessingCanvas.Instance.CloseCanvas();
        Time.timeScale = 0;
        ReinforceCanvas.Instance.OpenCanvas(() =>
        {
            Time.timeScale = 1;
        });
    }
}
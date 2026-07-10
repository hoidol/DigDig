using System;
using System.Collections.Generic;
using UnityEngine;

public class BlessingCanvas : CanvasUI<BlessingCanvas>
{

    public ReinforceButton reinforceButton;
    public DrawItemButton drawItemButton;
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        List<string> bulletDataKeys = new List<string>();
        foreach (var bulletStat in Player.Instance.statMgr.bulletStatDic.Values)
        {
            if (Player.Instance.weapon.bulletInventory.CheckHave(bulletStat.key) && bulletStat.lv < BulletData.MAX_LEVEL)
            {
                bulletDataKeys.Add(bulletStat.key);
            }
        }

        List<string> itemKeys = new List<string>();
        for (int i = 0; i < Player.Instance.itemInventory.curItems.Count; i++)
        {
            if (Player.Instance.statMgr.GetPlayerItemStat(Player.Instance.itemInventory.curItems[i].key).lv >= ItemData.MAX_LEVEL)
            {
                continue;
            }
            itemKeys.Add(Player.Instance.itemInventory.curItems[i].key);
        }
        int totalCanDrawCount = bulletDataKeys.Count + itemKeys.Count;
        reinforceButton.gameObject.SetActive(totalCanDrawCount > 0);


        #region  SelectItem
        if (Player.Instance.itemInventory.curItems.Count >= 5)
        {
            drawItemButton.gameObject.SetActive(false);
        }
        else
        {
            drawItemButton.gameObject.SetActive(false);
        }
        //List<ItemData> itemDatas = ItemManager.Instance.GetDrawItems(3);
        #endregion
    }

}

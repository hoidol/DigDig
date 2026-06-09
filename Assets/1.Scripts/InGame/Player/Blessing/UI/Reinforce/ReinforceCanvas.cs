using System;
using System.Collections.Generic;
using UnityEngine;

public class ReinforceCanvas : CanvasUI<ReinforceCanvas>
{
    public ReinforceBulletPanel reinforceBulletPanelPrefab;
    public ReinforceBulletPanel reinforceItemPanelPrefab;
    public RectTransform parentTr;
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        List<string> bulletDataKeys = new List<string>();
        foreach (var bulletStat in Player.Instance.statMgr.bulletStatDic.Values)
        {
            if (Player.Instance.weapon.bulletInventory.CheckHave(bulletStat.key) && bulletStat.lv >= BulletData.MAX_LEVEL)
            {
                continue;
            }

            bulletDataKeys.Add(bulletStat.key);
        }


        List<string> itemKeys = new List<string>();
        for (int i = 0; i < Player.Instance.itemInventory.curItems.Count; i++)
        {
            if (Player.Instance.statMgr.itemStatDic[Player.Instance.itemInventory.curItems[i].key].lv >= ItemData.MAX_LEVEL)
            {
                continue;
            }
            itemKeys.Add(Player.Instance.itemInventory.curItems[i].key);
        }

        int totalCanDrawCount = bulletDataKeys.Count + itemKeys.Count;
        
        int reinForceCount= Mathf.Clamp(UnityEngine.Random.Range(1,5),1,totalCanDrawCount);
    }
}

//어떤식으로 강화시킬것인가

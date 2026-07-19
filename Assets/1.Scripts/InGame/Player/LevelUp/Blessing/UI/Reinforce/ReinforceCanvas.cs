using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ReinforceCanvas : CanvasUI<ReinforceCanvas>
{
    public ReinforceBulletPanel reinforceBulletPanelPrefab;
    public ReinforceBulletPanel reinforceItemPanelPrefab;
    public RectTransform parentTr;
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        List<IReinforce> candidates = new List<IReinforce>();
        foreach (var bulletStat in Player.Instance.statMgr.bulletStatDic.Values)
        {
            // if (Player.Instance.weapon.bulletInventory.CheckHave(bulletStat.key) && bulletStat.lv >= BulletData.MAX_LEVEL)
            // {
            //     continue;
            // }

            candidates.Add(BulletManager.bullets[bulletStat.key]);
        }


        for (int i = 0; i < Player.Instance.itemInventory.curItems.Count; i++)
        {
            if (Player.Instance.statMgr.GetPlayerItemStat(Player.Instance.itemInventory.curItems[i].key).lv >= ItemData.MAX_LEVEL)
            {
                continue;
            }
            candidates.Add(Player.Instance.itemInventory.curItems[i]);
        }

        int totalCanDrawCount = candidates.Count;
        IReinforce[] reinforces = candidates.OrderBy(e => UnityEngine.Random.value).Take(3).ToArray();

        int reinForceCount = Mathf.Clamp(UnityEngine.Random.Range(1, 5), 1, totalCanDrawCount);
    }
}

//어떤식으로 강화시킬것인가

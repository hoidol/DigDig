using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ReinforceCanvas : CanvasUI<ReinforceCanvas>
{
    public ReinforcePanel reinforceBulletPanelPrefab;
    public ReinforcePanel reinforceItemPanelPrefab;
    public RectTransform parentTr;

    public List<ReinforcePanel> reinforcePanels = new();
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        for(int i = 0; i < reinforcePanels.Count; i++)
        {
            reinforcePanels[i].gameObject.SetActive(false);
        }

        List<IReinforce> candidates = new List<IReinforce>();
        int canLevelUpCount = 0;
        foreach (var bulletStat in Player.Instance.statMgr.bulletStatDic.Values)
        {
            if (Player.Instance.weapon.bulletInventory.CheckHave(bulletStat.key) && bulletStat.lv >= BulletData.MAX_LEVEL)
            {
                continue;
            }
            canLevelUpCount += BulletData.MAX_LEVEL - bulletStat.lv;
            candidates.Add(BulletManager.bullets[bulletStat.key]);
        }


        for (int i = 0; i < Player.Instance.itemInventory.curItems.Count; i++)
        {
            PlayerItemStat itemStat = Player.Instance.statMgr.GetPlayerItemStat(Player.Instance.itemInventory.curItems[i].key);
            if (itemStat.lv >= ItemData.MAX_LEVEL)
            {
                continue;
            }
            canLevelUpCount += ItemData.MAX_LEVEL - itemStat.lv;
            candidates.Add(Player.Instance.itemInventory.curItems[i]);
        }
        
        reinforceResults.Clear();
        int reinForceCount = Mathf.Clamp(UnityEngine.Random.Range(1, 5), 1, canLevelUpCount);
        while (reinForceCount > 0)
        {
            if (candidates.Count == 0) break; 

            IReinforce reinforce = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            if (reinforce.IsMaxLevel())
            {
                candidates.Remove(reinforce);
                continue;
            }
            int preLv = reinforce.GetLevel();

            if(reinforce.ReinforceType == ReinforceType.Bullet )
            {
                Player.Instance.LevelUpBullet(reinforce.Key);
            }
            else if(reinforce.ReinforceType == ReinforceType.Item)
            {
                Player.Instance.LevelUpItem(reinforce.Key);
            }
            ReinforceResult result = GetReinforceResult(reinforce.Key);
            ReinforcePanel panel = GetReinforcePanel(reinforce.ReinforceType);
            panel.SetReinforce(reinforce, preLv, preLv+1);
            result.lvUpCount++;
            reinForceCount--;
        }

    }
    List<ReinforceResult> reinforceResults = new List<ReinforceResult>();
    ReinforceResult GetReinforceResult(string key)
    {
        for(int i = 0; i < reinforceResults.Count; i++)
        {
            if(reinforceResults[i].key == key)
            {
                return reinforceResults[i];
            }
        }
        ReinforceResult result =  new ReinforceResult();
        result.key = key;
        reinforceResults.Add(result);
        return result;
    }

    ReinforcePanel GetReinforcePanel(ReinforceType type)
    {
        for(int i = 0; i < reinforcePanels.Count; i++)
        {
            if(reinforcePanels[i].gameObject.activeSelf)
            continue;
            if(reinforcePanels[i].reinforceType == type)
            {
                reinforcePanels[i].gameObject.SetActive(true);
                return reinforcePanels[i];
            }
        }
        
        ReinforcePanel prefab = type switch
            {
                ReinforceType.Bullet => reinforceBulletPanelPrefab,
                ReinforceType.Item   => reinforceItemPanelPrefab,
                _                    => throw new ArgumentOutOfRangeException(nameof(type))
            };
        
        ReinforcePanel panel = Instantiate(prefab, parentTr);

        reinforcePanels.Add(panel);
        return panel;
    }
}
public class ReinforceResult
{
    public string key;
    public ReinforceType reinforceType;
    public int lvUpCount;
}

//어떤식으로 강화시킬것인가

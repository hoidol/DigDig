using System.Collections.Generic;
using UnityEngine;

public class BlessingAddNormalBulletButton : BlessingButton
{
    public override void UpdateButton()
    {
        bool active = true;
        //아이템 획득 가능한지
        if (Player.Instance.itemInventory.curItems.Count < 5)
        {
            active = false;
        }

        //머지할게 있는지
        //  List<MergeBulletData> canMergeBulletDatas = Player.Instance.weapon.bulletInventory.GetCanMergeBulletData();
        // if(canMergeBulletDatas.Count > 0)
        // {
        //     active = false;    
        // }

        //강화할게 있는지
        List<string> bulletDataKeys = new List<string>();
        // foreach (var bulletStat in Player.Instance.statMgr.bulletStatDic.Values)
        // {
        //     if (Player.Instance.weapon.bulletInventory.CheckHave(bulletStat.key) && bulletStat.lv < BulletData.MAX_LEVEL)
        //     {
        //         active = false;
        //         break;
        //     }
        // }

        for (int i = 0; i < Player.Instance.itemInventory.curItems.Count; i++)
        {
            if (Player.Instance.statMgr.GetPlayerItemStat(Player.Instance.itemInventory.curItems[i].key).lv >= ItemData.MAX_LEVEL)
                continue;

            active = false;
            break;
        }

        gameObject.SetActive(active);


        titleText.text = string.Format(TranslateManager.GetText($"BlessingAddNormalBullet"), 5);
    }


    public override void OnClickedBtn()
    {
        // Player.Instance.weapon.AddBullet("Normal");
        // Player.Instance.weapon.AddBullet("Normal");
        // Player.Instance.weapon.AddBullet("Normal");
        // Player.Instance.weapon.AddBullet("Normal");
        // Player.Instance.weapon.AddBullet("Normal");


        BlessingCanvas.Instance.CloseCanvas();
    }
}
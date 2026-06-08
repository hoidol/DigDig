using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelUpCanvas : CanvasUI<LevelUpCanvas>
{
    //public LevelUpBonusPanel[] levelUpBonusPanels;

    public LevelUpBonusPanel addNormalBulletPanel;
    public LevelUpBonusPanel addBouncePanel;
    public AddSpecialBulletPanel addSpecialBulletPanel;
    public MergeBulletPanel mergeBulletPanel;

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        // if (levelUpBonusPanels == null || levelUpBonusPanels.Length <= 0)
        // {
        //     levelUpBonusPanels = GetComponentsInChildren<LevelUpBonusPanel>();
        // }

        Bullet normal = Player.Instance.weapon.bulletInventory.curBullets.Where(e => e.key == "Normal").FirstOrDefault();
        addSpecialBulletPanel.CanSelect(normal != null);


        List<MergeBulletData> canMergeBulletDatas = Player.Instance.weapon.bulletInventory.GetCanMergeBulletData();
        mergeBulletPanel.gameObject.SetActive(canMergeBulletDatas.Count <= 0);

    }
}
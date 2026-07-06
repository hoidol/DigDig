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

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

        string normal = Player.Instance.weapon.bulletInventory.curBullets.Where(e => e == "Normal").FirstOrDefault();
        addSpecialBulletPanel.CanSelect(normal != null);



    }
}
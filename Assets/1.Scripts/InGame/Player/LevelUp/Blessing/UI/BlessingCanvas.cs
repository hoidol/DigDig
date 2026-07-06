using System;
using System.Collections.Generic;
using UnityEngine;

public class BlessingCanvas : CanvasUI<BlessingCanvas>
{
    public BlessingButton[] blessings;
    public ReinforceButton reinforceButton;
    public DrawItemButton drawItemButton;
    public MergeBulletButton mergeBulletButton;
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        for(int i = 0; i < blessings.Length; i++)
        {
            blessings[i].UpdateButton();
        }

    }

}

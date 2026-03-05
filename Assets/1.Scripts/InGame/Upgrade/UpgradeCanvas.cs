using UnityEngine;
using System;
public class UpgradeCanvas : CanvasUI<UpgradeCanvas>
{
    public UpgradePanel[] upgradePanels;
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        UpdateCanvas();
    }
    public void UpdateCanvas()
    {
        for (int i = 0; i < upgradePanels.Length; i++)
        {
            upgradePanels[i].UpdatePanel();
        }
    }
}

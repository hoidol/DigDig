using System;
using UnityEngine;
using TMPro;
public class LifeFountainCanvas : CanvasUI<LifeFountainCanvas>
{

    public TMP_Text contextsText;
    LifeFountain lifeFountain;
    public void OpenCanvas(LifeFountain lifeFountain, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);

    }

    public void OnClickAccept()
    {
        float healAmount = Player.Instance.statMgr.MaxHp * 0.3f;
        Player.Instance.AddHp(healAmount);
        lifeFountain.Destroy();
        CloseCanvas();
    }

}
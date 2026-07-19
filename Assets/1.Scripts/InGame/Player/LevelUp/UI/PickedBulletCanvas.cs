using System;
using System.Collections.Generic;
using UnityEngine;

public class PickedBulletCanvas : CanvasUI<PickedBulletCanvas>
{
    // public PickedBulletPanel pickedBulletPanel;

    bool init = false;
    List<string> alreadyPicked = new List<string>();
    private void Init()
    {

        if (init)
            return;
        init = true;
        alreadyPicked.Clear();


    }
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        Init();
        BulletData pickedBulletData = BulletManager.Instance.DrawRandomBullet();
        // pickedBulletPanel.SetBulletData(pickedBulletData);
        if (!alreadyPicked.Contains(pickedBulletData.key))
        {
            alreadyPicked.Add(pickedBulletData.key);
        }
        // Player.Instance.weapon.AddBullet(pickedBulletData);

    }
}

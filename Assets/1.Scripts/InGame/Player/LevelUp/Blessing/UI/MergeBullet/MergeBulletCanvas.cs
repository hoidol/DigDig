using UnityEngine;

using System;
using System.Collections.Generic;
public class MergeBulletCanvas : CanvasUI<MergeBulletCanvas>
{
    //public MergeBulletPanel mergeBulletPanelPrefab;
    PoolingSystem<MergeBulletPanel> poolingSystem;

    void Awake()
    {
        poolingSystem = new PoolingSystem<MergeBulletPanel>();
        poolingSystem.SetPrefab("UI/MergeBulletPanel");

    }
    
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        UpdateContainer();
    }

    public void UpdateContainer()
    {
        poolingSystem.ReturnAll();
        List<MergeBulletData> canMergeBulletDatas = Player.Instance.weapon.bulletInventory.GetCanMergeBulletData();
        for (int i = 0; i < canMergeBulletDatas.Count; i++)
        {
            MergeBulletPanel panel = poolingSystem.GetObject<MergeBulletPanel>();
            panel.SetMergeBulletData(canMergeBulletDatas[i]);
        }
    }
    public void Selected(MergeBulletData mergeBulletData)
    {
        Player.Instance.MergeBullet(mergeBulletData);
        CloseCanvas();
    }
}

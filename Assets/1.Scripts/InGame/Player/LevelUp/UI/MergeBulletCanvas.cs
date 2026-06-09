using UnityEngine;

using System;
using System.Collections.Generic;
public class MergeBulletCanvas : CanvasUI<MergeBulletCanvas>
{
    public MergeBulletPanel mergeBulletPanelPrefab;
    PoolingSystem<MergeBulletPanel> poolingSystem;

    void Awake()
    {
        poolingSystem = new PoolingSystem<MergeBulletPanel>();
        poolingSystem.SetPrefab("UI/MergeBulletPanel");
    }
    int remainMergeCount = 0;
    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        remainMergeCount =2;
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
        remainMergeCount--;
        BulletManager.Instance.Merge(mergeBulletData);
        UpdateContainer();
        if(remainMergeCount <= 0)
        {
            CloseCanvas();
        }
    }
}

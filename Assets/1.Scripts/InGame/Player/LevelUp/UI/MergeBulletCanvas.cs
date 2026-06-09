using UnityEngine;

using System;
using System.Collections.Generic;
public class MergeBulletCanvas : CanvasUI<MergeBulletCanvas>
{
    public MergeBulletPanel mergeBulletPanelPrefab;
    [SerializeField] PoolingSystem pool;

    void Awake()
    {
        pool.SetObject(mergeBulletPanelPrefab);
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
         pool.ReturnAll();
        List<MergeBulletData> canMergeBulletDatas = Player.Instance.weapon.bulletInventory.GetCanMergeBulletData();
        for (int i = 0; i < canMergeBulletDatas.Count; i++)
        {
            MergeBulletPanel panel = pool.GetObject<MergeBulletPanel>();
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

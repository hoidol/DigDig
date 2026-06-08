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

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        pool.ReturnAll();
        List<MergeBulletData> canMergeBulletDatas = Player.Instance.weapon.bulletInventory.GetCanMergeBulletData();
        for (int i = 0; i < canMergeBulletDatas.Count; i++)
        {
            MergeBulletPanel panel = pool.GetObject<MergeBulletPanel>();
            panel.SetMergeBulletData(canMergeBulletDatas[i]);
        }
    }

}

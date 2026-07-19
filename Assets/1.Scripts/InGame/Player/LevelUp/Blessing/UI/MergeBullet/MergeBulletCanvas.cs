using UnityEngine;

using System;
using System.Collections.Generic;
using UnityEngine.UI;
public class MergeBulletCanvas : CanvasUI<MergeBulletCanvas>
{
    [SerializeField] HorizontalLayoutGroup horizontalLayoutGroup;
    [SerializeField] BulletUI bulletUIPrefab; // 170, 170
    List<BulletUI> bulletUIList = new List<BulletUI>();
    [SerializeField] RectTransform bulletUIParentTr;
    PoolingSystem<MergeBulletPanel> poolingSystem;
    PoolingSystem<BulletUI> bulletUIPoolingSystem;
    float defaultSpacing;

    void Awake()
    {
        poolingSystem = new PoolingSystem<MergeBulletPanel>();
        poolingSystem.SetPrefab("UI/MergeBulletPanel");

        bulletUIPoolingSystem = new PoolingSystem<BulletUI>();
        bulletUIPoolingSystem.SetPrefab(bulletUIPrefab);

        defaultSpacing = horizontalLayoutGroup.spacing;
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        UpdateContainer();
    }

    public void UpdateContainer()
    {
        poolingSystem.ReturnAll();

        bulletUIPoolingSystem.ReturnAll();
        bulletUIList.Clear();
        // List<string> curBullets = Player.Instance.weapon.bulletInventory.curBullets;
        // for (int i = 0; i < curBullets.Count; i++)
        // {
        //     BulletUI bulletUI = bulletUIPoolingSystem.GetObject<BulletUI>();
        //     bulletUI.transform.SetParent(bulletUIParentTr);
        //     bulletUI.SetBulletData(BulletData.GetBulletData(curBullets[i]));
        //     bulletUIList.Add(bulletUI);
        // }
        AdjustBulletUISpacing();

        // List<MergeBulletData> canMergeBulletDatas = Player.Instance.weapon.bulletInventory.GetCanMergeBulletData();
        // for (int i = 0; i < canMergeBulletDatas.Count; i++)
        // {
        //     MergeBulletPanel panel = poolingSystem.GetObject<MergeBulletPanel>();
        //     panel.SetMergeBulletData(canMergeBulletDatas[i]);
        // }
    }
    void AdjustBulletUISpacing()
    {
        int count = bulletUIList.Count;
        if (count <= 1)
        {
            horizontalLayoutGroup.spacing = defaultSpacing;
            return;
        }

        float itemWidth = ((RectTransform)bulletUIPrefab.transform).rect.width;
        float availableWidth = bulletUIParentTr.rect.width - horizontalLayoutGroup.padding.left - horizontalLayoutGroup.padding.right;
        float fitSpacing = (availableWidth - itemWidth * count) / (count - 1);
        horizontalLayoutGroup.spacing = Mathf.Clamp(fitSpacing, 0f, defaultSpacing);
    }

    public void Selected(MergeBulletData mergeBulletData)
    {
        Player.Instance.MergeBullet(mergeBulletData);
        CloseCanvas();
    }
}

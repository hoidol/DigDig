using System;
using System.Collections.Generic;
using UnityEngine;

public class BulletManageCanvas :  CanvasUI<BulletManageCanvas>
{
    [SerializeField] BulletEquiptPanel[] equiptPanels;

    [SerializeField] BulletEntryPanel bulletEntryPanelPrefab;
    [SerializeField] RectTransform parentTr;
    [SerializeField] List<BulletEntryPanel> bulletEntryPanels = new();


    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        UpdateCanvas();
    }
    public void UpdateCanvas()
    {
        for(int i = 0; i < UserManager.Instance.userBulletManager.userBulletData.equiptedBullets.Length; i++)
        {
            string key = UserManager.Instance.userBulletManager.userBulletData.equiptedBullets[i].key;
            equiptPanels[i].SetBullet(key,i);
        }

        for(int i = 0; i < bulletEntryPanels.Count; i++)
            bulletEntryPanels[i].gameObject.SetActive(false);
        
        for(int i = 0; i < BulletManager.Instance.bulletDatas.Length; i++)
        {
            BulletEntryPanel entryPanel = GetBulletEntryPanel();
            entryPanel.SetBullet(BulletManager.Instance.bulletDatas[i].key);            
        }
    }

    BulletEntryPanel GetBulletEntryPanel()
    {
        for(int i = 0; i < bulletEntryPanels.Count; i++)
        {
            if(bulletEntryPanels[i].gameObject.activeSelf)
                continue;
            bulletEntryPanels[i].gameObject.SetActive(true);
            return bulletEntryPanels[i];
        }
        BulletEntryPanel bulletEntryPanel = Instantiate(bulletEntryPanelPrefab,parentTr);
        bulletEntryPanels.Add(bulletEntryPanel);
        return bulletEntryPanel;
    }


}
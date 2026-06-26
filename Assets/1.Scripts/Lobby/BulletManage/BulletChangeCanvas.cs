using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BulletChangeCanvas :  CanvasUI<BulletChangeCanvas>
{
    [SerializeField] BulletChangeButton[] changeButtons;

    [SerializeField] BulletEntryPanel bulletEntryPanelPrefab;
    [SerializeField] RectTransform parentTr;
    [SerializeField] List<BulletEntryPanel> bulletEntryPanels = new();


    void Awake()
    {
        for(int i = 0; i < changeButtons.Length; i++)
        {
            changeButtons[i].Init(i);
        }
    }
    
    void OnEnable()
    {
        UpdateCanvas();
    }

    void UpdateCanvas()
    {
        for(int i = 0; i < UserManager.Instance.userBulletManager.userBulletData.equiptedBullets.Length; i++)
        {
            string key = UserManager.Instance.userBulletManager.userBulletData.equiptedBullets[i].key;
            changeButtons[i].SetBullet(key);
        }
    }

}
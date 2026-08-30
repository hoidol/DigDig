using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BulletInfoCanvas : CanvasUI<BulletInfoCanvas>
{
    //강화 시 능력치
    [SerializeField] BulletData bulletData;
    [SerializeField] Image thumImage;
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text descText;
    [SerializeField] int lv;
    [SerializeField] GameObject nextLvButton;
    [SerializeField] GameObject preLvButton;

    [SerializeField] BulletDisplayPanel bulletDisplayPanelPrefab;
    [SerializeField] List<BulletDisplayPanel> bulletDisplayPanelList = new List<BulletDisplayPanel>();
    [SerializeField] RectTransform parentTr;


    public void OpenCanvas(string key, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        bulletData = BulletData.GetBulletData(key);

        titleText.text = bulletData.Title;
        thumImage.sprite = bulletData.thumbnail;
        lv = 1;

        RefreshMergeBulletPanels();
        UpdateCanvas();
    }

    private void RefreshMergeBulletPanels()
    {
        // string[] keys = bulletData.mergeBulletKeys;

        // for (int i = 0; i < bulletDisplayPanelList.Count; i++)
        //     bulletDisplayPanelList[i].gameObject.SetActive(false);

        // for (int i = 0; i < keys.Length; i++)
        // {
        //     BulletDisplayPanel panel;
        //     if (i < bulletDisplayPanelList.Count)
        //     {
        //         panel = bulletDisplayPanelList[i];
        //     }
        //     else
        //     {
        //         panel = Instantiate(bulletDisplayPanelPrefab, parentTr);
        //         bulletDisplayPanelList.Add(panel);
        //     }
        //     panel.gameObject.SetActive(true);
        //     panel.SetBulletData(keys[i]);
        // }
    }

    public void UpdateCanvas()
    {
        // descText.text = bulletData.GetDescription(lv);
        preLvButton.SetActive(false);
        nextLvButton.SetActive(false);
        if (lv > 1)
        {
            preLvButton.SetActive(true);
        }
        if (lv < BulletData.MAX_LEVEL)
        {
            nextLvButton.SetActive(true);
        }

    }

    public void OnClickedNextLevel()
    {
        lv++;
        UpdateCanvas();
    }

    public void OnClickedPreLevel()
    {
        lv--;
        UpdateCanvas();
    }

}

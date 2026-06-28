using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BulletChangeCanvas : CanvasUI<BulletChangeCanvas>
{
    [SerializeField] BulletChangeButton[] changeButtons;


    void Awake()
    {
        for (int i = 0; i < changeButtons.Length; i++)
        {
            changeButtons[i].Init(i);
        }
    }

    void OnEnable()
    {
        UpdateCanvas();
    }
    public void OpenCanvas(UserBullet userBullet, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
    }

    void UpdateCanvas()
    {
        for (int i = 0; i < UserManager.Instance.userBulletManager.userBulletData.equiptedBullets.Length; i++)
        {
            string key = UserManager.Instance.userBulletManager.userBulletData.equiptedBullets[i].key;
            changeButtons[i].SetBullet(key);
        }
    }

}